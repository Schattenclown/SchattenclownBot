using System.Linq;
using System.Text;

namespace SchattenclownBot.Model.HelpClasses;

internal class SpecialChars
{
	public static string RemoveSpecialCharacters(string inputString)
	{
		StringBuilder stringBuilder = new();
		foreach (char c in inputString.Where(c => c < 255))
		{
			stringBuilder.Append(c);
		}

		return stringBuilder.ToString();
	}
}
