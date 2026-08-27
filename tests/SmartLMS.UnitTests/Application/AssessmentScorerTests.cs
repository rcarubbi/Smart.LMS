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
        var question = new SingleChoiceQuestion("Pick A", 5);
        question.SetRightOption(OptionA);
        question.Options.Add(new QuestionOption("A", 0) { Id = OptionA });
        question.Options.Add(new QuestionOption("B", 1) { Id = OptionB });

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
        var question = new MultipleChoiceQuestion("Pick A and C", 6);
        foreach (var id in new[] { OptionA, OptionB, OptionC })
        {
            question.Options.Add(new QuestionOption(id.ToString(), 0) { Id = id });
        }
        foreach (var id in new[] { OptionA, OptionC })
        {
            question.RightOptions.Add(new QuestionOption("Right", 0) { Id = id });
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
        var question = new ReorderingQuestion("Order the steps", 4);
        // Correct order is B (order 1), A (order 2), C (order 3).
        question.Options.Add(new QuestionOption("Step A", 2) { Id = OptionA });
        question.Options.Add(new QuestionOption("Step B", 1) { Id = OptionB });
        question.Options.Add(new QuestionOption("Step C", 3) { Id = OptionC });

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
        var single = new SingleChoiceQuestion("S", 3);
        single.SetRightOption(OptionA);
        single.Options.Add(new QuestionOption("A", 0) { Id = OptionA });

        var reorder = new ReorderingQuestion("R", 2);
        reorder.Options.Add(new QuestionOption("A", 0) { Id = OptionA });

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
