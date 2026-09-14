using SmartLMS.Domain.Entities.Assessments;

namespace SmartLMS.UnitTests.Application;

public class AssessmentScoringTests
{
    private static readonly Guid OptionA = Guid.NewGuid();
    private static readonly Guid OptionB = Guid.NewGuid();
    private static readonly Guid OptionC = Guid.NewGuid();

    [Test]
    public async Task Single_choice_scores_only_when_right_option_picked()
    {
        var assessment = new Assessment(Guid.NewGuid(), minimumScore: 5);
        var question = new SingleChoiceQuestion("Pick A", 5);
        question.SetRightOption(OptionA);
        question.Options.Add(new QuestionOption("A", 0) { Id = OptionA });
        question.Options.Add(new QuestionOption("B", 1) { Id = OptionB });
        assessment.AddQuestion(question);

        var right = assessment.Score(new Dictionary<Guid, IReadOnlyList<Guid>>
        {
            [question.Id] = [OptionA],
        });
        var wrong = assessment.Score(new Dictionary<Guid, IReadOnlyList<Guid>>
        {
            [question.Id] = [OptionB],
        });

        using (Assert.Multiple())
        {
            await Assert.That(right.Total).IsEqualTo(5);
            await Assert.That(right.Passed).IsTrue();
            await Assert.That(right.PerQuestion[0].Correct).IsTrue();
            await Assert.That(right.PerQuestion[0].Awarded).IsEqualTo(5);
            await Assert.That(wrong.Total).IsEqualTo(0);
            await Assert.That(wrong.Passed).IsFalse();
        }
    }

    [Test]
    public async Task Multiple_choice_requires_exact_set_match()
    {
        var assessment = new Assessment(Guid.NewGuid(), minimumScore: 6);
        var question = new MultipleChoiceQuestion("Pick A and C", 6);
        foreach (var id in new[] { OptionA, OptionB, OptionC })
        {
            question.Options.Add(new QuestionOption(id.ToString(), 0) { Id = id });
        }
        foreach (var id in new[] { OptionA, OptionC })
        {
            question.RightOptions.Add(new QuestionOption("Right", 0) { Id = id });
        }
        assessment.AddQuestion(question);

        Dictionary<Guid, IReadOnlyList<Guid>> Answers(params Guid[] ids) =>
            new() { [question.Id] = ids };

        var full = assessment.Score(Answers(OptionC, OptionA));
        var partial = assessment.Score(Answers(OptionA));
        var extra = assessment.Score(Answers(OptionA, OptionB, OptionC));

        using (Assert.Multiple())
        {
            await Assert.That(full.Total).IsEqualTo(6);
            await Assert.That(full.PerQuestion[0].Correct).IsTrue();
            await Assert.That(partial.Total).IsEqualTo(0);
            await Assert.That(extra.Total).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Reordering_requires_exact_sequence_of_ordered_options()
    {
        var assessment = new Assessment(Guid.NewGuid(), minimumScore: 4);
        var question = new ReorderingQuestion("Order the steps", 4);
        // Correct order is B (order 1), A (order 2), C (order 3).
        question.Options.Add(new QuestionOption("Step A", 2) { Id = OptionA });
        question.Options.Add(new QuestionOption("Step B", 1) { Id = OptionB });
        question.Options.Add(new QuestionOption("Step C", 3) { Id = OptionC });
        assessment.AddQuestion(question);

        var rightSequence = new[] { OptionB, OptionA, OptionC };

        var right = assessment.Score(new Dictionary<Guid, IReadOnlyList<Guid>>
        {
            [question.Id] = rightSequence,
        });
        var wrong = assessment.Score(new Dictionary<Guid, IReadOnlyList<Guid>>
        {
            [question.Id] = [OptionA, OptionB, OptionC],
        });

        using (Assert.Multiple())
        {
            await Assert.That(right.Total).IsEqualTo(4);
            await Assert.That(right.PerQuestion[0].Correct).IsTrue();
            await Assert.That(wrong.Total).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Unanswered_questions_score_zero_and_totals_accumulate()
    {
        var assessment = new Assessment(Guid.NewGuid(), minimumScore: 5);
        var single = new SingleChoiceQuestion("S", 3);
        single.SetRightOption(OptionA);
        single.Options.Add(new QuestionOption("A", 0) { Id = OptionA });
        assessment.AddQuestion(single);

        var reorder = new ReorderingQuestion("R", 2);
        reorder.Options.Add(new QuestionOption("A", 0) { Id = OptionA });
        assessment.AddQuestion(reorder);

        var scored = assessment.Score(new Dictionary<Guid, IReadOnlyList<Guid>>
        {
            [single.Id] = [OptionA],
            // reorder unanswered
        });

        using (Assert.Multiple())
        {
            await Assert.That(scored.Total).IsEqualTo(3);
            await Assert.That(scored.Passed).IsFalse();
            await Assert.That(scored.PerQuestion[1].Correct).IsFalse();
        }
    }
}
