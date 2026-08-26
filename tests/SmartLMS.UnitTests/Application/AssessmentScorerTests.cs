using SmartLMS.Application.Assessments;
using SmartLMS.Domain.Entities.Assessments;

namespace SmartLMS.UnitTests.Application;

public class AssessmentScorerTests
{
    private static readonly Guid OptionA = Guid.NewGuid();
    private static readonly Guid OptionB = Guid.NewGuid();
    private static readonly Guid OptionC = Guid.NewGuid();

    [Test]
    public async Task Single_choice_scores_only_when_right_option_picked()
    {
        var question = new SingleChoiceQuestion
        {
            Description = "Pick A",
            Score = 5,
            RightOptionId = OptionA,
        };
        question.Options.Add(new QuestionOption { Id = OptionA, Text = "A", Order = 0 });
        question.Options.Add(new QuestionOption { Id = OptionB, Text = "B", Order = 1 });

        var (scoreRight, resultsRight) = AssessmentScorer.Score([(question, new QuestionSubmission(question.Id, [OptionA]))]);
        var (scoreWrong, _) = AssessmentScorer.Score([(question, new QuestionSubmission(question.Id, [OptionB]))]);

        using (Assert.Multiple())
        {
            await Assert.That(scoreRight).IsEqualTo(5);
            await Assert.That(resultsRight[0].Correct).IsTrue();
            await Assert.That(scoreWrong).IsEqualTo(0);
            await Assert.That(resultsRight[0].ScoreAwarded).IsEqualTo(5);
        }
    }

    [Test]
    public async Task Multiple_choice_requires_exact_set_match()
    {
        var question = new MultipleChoiceQuestion { Description = "Pick A and C", Score = 6 };
        foreach (var id in new[] { OptionA, OptionB, OptionC })
        {
            var option = new QuestionOption { Id = id, Text = id.ToString(), Order = 0 };
            question.Options.Add(option);
        }
        foreach (var id in new[] { OptionA, OptionC })
        {
            question.RightOptions.Add(new QuestionOption { Id = id });
        }

        var (full, fullResults) = AssessmentScorer.Score([(question, new QuestionSubmission(question.Id, [OptionC, OptionA]))]);
        var (partial, _) = AssessmentScorer.Score([(question, new QuestionSubmission(question.Id, [OptionA]))]);
        var (extra, _) = AssessmentScorer.Score([(question, new QuestionSubmission(question.Id, [OptionA, OptionB, OptionC]))]);

        using (Assert.Multiple())
        {
            await Assert.That(full).IsEqualTo(6);
            await Assert.That(fullResults[0].Correct).IsTrue();
            await Assert.That(partial).IsEqualTo(0);
            await Assert.That(extra).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Reordering_requires_exact_sequence_of_ordered_options()
    {
        var question = new ReorderingQuestion { Description = "Order the steps", Score = 4 };
        // Correct order is B (order 1), A (order 2), C (order 3).
        question.Options.Add(new QuestionOption { Id = OptionA, Text = "Step A", Order = 2 });
        question.Options.Add(new QuestionOption { Id = OptionB, Text = "Step B", Order = 1 });
        question.Options.Add(new QuestionOption { Id = OptionC, Text = "Step C", Order = 3 });

        var rightSequence = new[] { OptionB, OptionA, OptionC };

        var (right, rightResults) = AssessmentScorer.Score([(question, new QuestionSubmission(question.Id, rightSequence))]);
        var (wrong, _) = AssessmentScorer.Score([(question, new QuestionSubmission(question.Id, [OptionA, OptionB, OptionC]))]);

        using (Assert.Multiple())
        {
            await Assert.That(right).IsEqualTo(4);
            await Assert.That(rightResults[0].Correct).IsTrue();
            await Assert.That(wrong).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Unanswered_questions_score_zero_and_totals_accumulate()
    {
        var single = new SingleChoiceQuestion { Description = "S", Score = 3, RightOptionId = OptionA };
        single.Options.Add(new QuestionOption { Id = OptionA, Order = 0 });

        var reorder = new ReorderingQuestion { Description = "R", Score = 2 };
        reorder.Options.Add(new QuestionOption { Id = OptionA, Order = 0 });

        var (score, results) = AssessmentScorer.Score(
        [
            (single, new QuestionSubmission(single.Id, [OptionA])),
            (reorder, null), // unanswered
        ]);

        using (Assert.Multiple())
        {
            await Assert.That(score).IsEqualTo(3);
            await Assert.That(results[1].Correct).IsFalse();
        }
    }
}
