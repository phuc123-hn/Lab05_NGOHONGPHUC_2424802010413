using System.Globalization;
using System.Text;

namespace PricingService.Domain;

/// <summary>
/// Bộ tính biểu thức nhỏ cho các luật giá, ví dụ:
/// (NUM_OF_ADULTS) * (DESTINATION == "EUR" ? 26.00M : 34.00M)
/// Hỗ trợ: số (hậu tố M tùy chọn), chuỗi, true/false, biến, + - * /, so sánh, && ||, !, ?:, ngoặc.
/// </summary>
public static class ExpressionEvaluator
{
    public static decimal EvaluateDecimal(string expression, IDictionary<string, object> variables)
    {
        var result = Evaluate(expression, variables);
        return result is decimal d
            ? d
            : throw new InvalidOperationException($"Biểu thức '{expression}' phải trả về số.");
    }

    public static bool EvaluateBool(string expression, IDictionary<string, object> variables)
    {
        var result = Evaluate(expression, variables);
        return result is bool b
            ? b
            : throw new InvalidOperationException($"Biểu thức '{expression}' phải trả về true/false.");
    }

    public static object Evaluate(string expression, IDictionary<string, object> variables)
    {
        var parser = new Parser(expression, variables);
        return parser.Parse();
    }

    private sealed class Parser
    {
        private readonly string text;
        private readonly IDictionary<string, object> variables;
        private int pos;

        public Parser(string text, IDictionary<string, object> variables)
        {
            this.text = text;
            this.variables = variables;
        }

        public object Parse()
        {
            var value = Ternary();
            SkipWhite();
            if (pos < text.Length)
                throw Error($"Ký tự không hợp lệ '{text[pos]}'");
            return value;
        }

        private object Ternary()
        {
            var condition = Or();
            SkipWhite();
            if (!Match("?")) return condition;

            var whenTrue = Ternary();
            SkipWhite();
            if (!Match(":")) throw Error("Thiếu dấu ':'");
            var whenFalse = Ternary();
            return AsBool(condition) ? whenTrue : whenFalse;
        }

        private object Or()
        {
            var left = And();
            while (true)
            {
                SkipWhite();
                if (!Match("||")) return left;
                var right = And();
                left = AsBool(left) || AsBool(right);
            }
        }

        private object And()
        {
            var left = Equality();
            while (true)
            {
                SkipWhite();
                if (!Match("&&")) return left;
                var right = Equality();
                left = AsBool(left) && AsBool(right);
            }
        }

        private object Equality()
        {
            var left = Relational();
            while (true)
            {
                SkipWhite();
                if (Match("==")) left = AreEqual(left, Relational());
                else if (Match("!=")) left = !AreEqual(left, Relational());
                else return left;
            }
        }

        private object Relational()
        {
            var left = Additive();
            while (true)
            {
                SkipWhite();
                if (Match("<=")) left = AsDecimal(left) <= AsDecimal(Additive());
                else if (Match(">=")) left = AsDecimal(left) >= AsDecimal(Additive());
                else if (Match("<")) left = AsDecimal(left) < AsDecimal(Additive());
                else if (Match(">")) left = AsDecimal(left) > AsDecimal(Additive());
                else return left;
            }
        }

        private object Additive()
        {
            var left = Multiplicative();
            while (true)
            {
                SkipWhite();
                if (Match("+")) left = AsDecimal(left) + AsDecimal(Multiplicative());
                else if (Match("-")) left = AsDecimal(left) - AsDecimal(Multiplicative());
                else return left;
            }
        }

        private object Multiplicative()
        {
            var left = Unary();
            while (true)
            {
                SkipWhite();
                if (Match("*")) left = AsDecimal(left) * AsDecimal(Unary());
                else if (Match("/")) left = AsDecimal(left) / AsDecimal(Unary());
                else return left;
            }
        }

        private object Unary()
        {
            SkipWhite();
            if (Match("-")) return -AsDecimal(Unary());
            if (Peek() == '!' && Peek(1) != '=')
            {
                pos++;
                return !AsBool(Unary());
            }

            return Primary();
        }

        private object Primary()
        {
            SkipWhite();
            if (pos >= text.Length) throw Error("Biểu thức kết thúc đột ngột");

            var c = text[pos];

            if (c == '(')
            {
                pos++;
                var inner = Ternary();
                SkipWhite();
                if (!Match(")")) throw Error("Thiếu dấu ')'");
                return inner;
            }

            if (c == '"') return ReadString();
            if (char.IsDigit(c) || c == '.') return ReadNumber();
            if (char.IsLetter(c) || c == '_') return ReadIdentifier();

            throw Error($"Ký tự không hợp lệ '{c}'");
        }

        private object ReadString()
        {
            pos++; // dấu "
            var sb = new StringBuilder();
            while (pos < text.Length && text[pos] != '"')
            {
                if (text[pos] == '\\' && pos + 1 < text.Length) pos++;
                sb.Append(text[pos++]);
            }

            if (pos >= text.Length) throw Error("Chuỗi thiếu dấu \"");
            pos++; // dấu " đóng
            return sb.ToString();
        }

        private object ReadNumber()
        {
            var start = pos;
            while (pos < text.Length && (char.IsDigit(text[pos]) || text[pos] == '.')) pos++;
            var number = text[start..pos];
            if (pos < text.Length && (text[pos] == 'M' || text[pos] == 'm')) pos++;
            return decimal.Parse(number, NumberStyles.Number, CultureInfo.InvariantCulture);
        }

        private object ReadIdentifier()
        {
            var start = pos;
            while (pos < text.Length && (char.IsLetterOrDigit(text[pos]) || text[pos] == '_')) pos++;
            var name = text[start..pos];

            if (name == "true") return true;
            if (name == "false") return false;

            if (!variables.TryGetValue(name, out var value) || value is null)
                throw new InvalidOperationException($"Biểu thức '{text}' dùng biến '{name}' chưa có giá trị.");

            return value switch
            {
                string or bool => value,
                IConvertible convertible => convertible.ToDecimal(CultureInfo.InvariantCulture),
                _ => value
            };
        }

        private static bool AreEqual(object left, object right)
        {
            if (left is decimal l && right is decimal r) return l == r;
            return Equals(left, right);
        }

        private decimal AsDecimal(object value) =>
            value is decimal d ? d : throw Error("Cần một giá trị số");

        private bool AsBool(object value) =>
            value is bool b ? b : throw Error("Cần một giá trị true/false");

        private bool Match(string token)
        {
            if (string.CompareOrdinal(text, pos, token, 0, token.Length) != 0) return false;
            pos += token.Length;
            return true;
        }

        private char Peek(int offset = 0) => pos + offset < text.Length ? text[pos + offset] : '\0';

        private void SkipWhite()
        {
            while (pos < text.Length && char.IsWhiteSpace(text[pos])) pos++;
        }

        private InvalidOperationException Error(string message) =>
            new($"Lỗi biểu thức '{text}' tại vị trí {pos}: {message}");
    }
}
