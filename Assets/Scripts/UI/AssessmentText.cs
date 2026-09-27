public static class AssessmentText
{
    public static string Format(AssessmentResult r)
    {
        if (r == null) return "";
        var text = new System.Text.StringBuilder();
        text.AppendLine($"Оценка: {r.score}");
        text.AppendLine($"Безопасность: {r.safetyScore} · Лояльность: {r.loyaltyScore}");
        if (!string.IsNullOrWhiteSpace(r.summary)) text.AppendLine("\n" + r.summary);
        if (r.strengths != null && r.strengths.Length > 0)
            text.AppendLine("\nЧто получилось:\n• " + string.Join("\n• ", r.strengths));
        if (r.mistakes != null && r.mistakes.Length > 0)
            text.AppendLine("\nНа что обратить внимание:\n• " + string.Join("\n• ", r.mistakes));
        if (r.criteria != null)
            foreach (var criterion in r.criteria)
                if (criterion != null)
                    text.AppendLine($"\n{criterion.criterion}: {criterion.score}\n{criterion.comment}");

        return text.ToString().Trim();
    }
}
