using SmartLMS.Application.Assessments;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Entities.Assessments;

namespace SmartLMS.UnitTests.Application;

public class AssessmentUpdateTests
{
    private static QuestionInput SingleInput(string description = "Q?", int score = 3) => new(
        "SingleChoice",
        description,
        score,
        [new OptionInput("A", 0), new OptionInput("B", 1)],
        SingleChoiceRightOrder: 0,
        MultipleChoiceRightOrders: []);

    [Test]
    public async Task UpdateValidator_rejects_empty_questions_and_negative_minimum()
    {
        var validator = new UpdateAssessmentValidator();

        var empty = await validator.ValidateAsync(
            new UpdateAssessmentCommand(Guid.NewGuid(), new UpdateAssessmentInput(0, [])));
        var negative = await validator.ValidateAsync(
            new UpdateAssessmentCommand(Guid.NewGuid(), new UpdateAssessmentInput(-1, [SingleInput()])));
        var unknownType = await validator.ValidateAsync(
            new UpdateAssessmentCommand(Guid.NewGuid(), new UpdateAssessmentInput(
                0, [SingleInput() with { Type = "Nope" }])));
        var outOfRange = await validator.ValidateAsync(
            new UpdateAssessmentCommand(Guid.NewGuid(), new UpdateAssessmentInput(
                0, [SingleInput() with { SingleChoiceRightOrder = 9 }])));
        var valid = await validator.ValidateAsync(
            new UpdateAssessmentCommand(Guid.NewGuid(), new UpdateAssessmentInput(5, [SingleInput()])));

        using (Assert.Multiple())
        {
            await Assert.That(empty.IsValid).IsFalse();
            await Assert.That(negative.IsValid).IsFalse();
            await Assert.That(unknownType.IsValid).IsFalse();
            await Assert.That(outOfRange.IsValid).IsFalse();
            await Assert.That(valid.IsValid).IsTrue();
        }
    }

    [Test]
    public async Task Assessment_is_aggregate_root_with_no_events_by_default()
    {
        var assessment = new Assessment(Guid.NewGuid(), minimumScore: 5);

        using (Assert.Multiple())
        {
            await Assert.That(assessment is AggregateRoot).IsTrue();
            await Assert.That(assessment.GetDomainEvents()).IsEmpty();
        }
    }

    [Test]
    public async Task ClearQuestions_empties_collection_for_rebuild()
    {
        var assessment = new Assessment(Guid.NewGuid(), minimumScore: 5);
        var question = new SingleChoiceQuestion("Q", 2);
        question.AttachAssessment(assessment);
        assessment.AddQuestion(question);

        assessment.ClearQuestions();

        await Assert.That(assessment.Questions).IsEmpty();
    }

    [Test]
    public async Task ClearRightOption_resets_single_choice_answer()
    {
        var question = new SingleChoiceQuestion("Q", 2);
        var optionId = Guid.NewGuid();
        question.SetRightOption(optionId);

        question.ClearRightOption();

        using (Assert.Multiple())
        {
            await Assert.That(question.RightOptionId).IsNull();
            await Assert.That(question.RightOption).IsNull();
        }
    }

    [Test]
    public async Task ClearRightOptions_empties_multiple_choice_answers()
    {
        var question = new MultipleChoiceQuestion("Q", 2);
        var option = new QuestionOption("A", 0);
        question.AddRightOption(option);

        question.ClearRightOptions();

        await Assert.That(question.RightOptions).IsEmpty();
    }

    [Test]
    public async Task Factory_builds_typed_questions_with_ordered_options()
    {
        var single = AssessmentQuestionFactory.Create(SingleInput("S?", 4));
        var multi = AssessmentQuestionFactory.Create(new QuestionInput(
            "MultipleChoice", "M?", 4,
            [new OptionInput("X", 1), new OptionInput("Y", 0)],
            SingleChoiceRightOrder: null,
            MultipleChoiceRightOrders: [0, 1]));
        var reorder = AssessmentQuestionFactory.Create(new QuestionInput(
            "Reordering", "R?", 2,
            [new OptionInput("B", 1), new OptionInput("A", 0)],
            SingleChoiceRightOrder: null,
            MultipleChoiceRightOrders: []));

        using (Assert.Multiple())
        {
            await Assert.That(single).IsTypeOf<SingleChoiceQuestion>();
            await Assert.That(multi).IsTypeOf<MultipleChoiceQuestion>();
            await Assert.That(reorder).IsTypeOf<ReorderingQuestion>();
            await Assert.That(single.Options.Select(o => o.Order)).IsEquivalentTo([0, 1]);
            await Assert.That(reorder.Options.Select(o => o.Text)).IsEquivalentTo(["A", "B"]);
        }
    }

    [Test]
    public async Task WireRightAnswers_returns_validation_for_out_of_range_index()
    {
        var question = new SingleChoiceQuestion("Q", 1);
        question.AddOption("Only", 0);

        var result = AssessmentQuestionFactory.WireRightAnswers(
            [new PendingQuestion(question, 5, [])]);

        using (Assert.Multiple())
        {
            await Assert.That(result.IsError).IsTrue();
            await Assert.That(result.FirstError.Type).IsEqualTo(global::ErrorOr.ErrorType.Validation);
        }
    }
}
