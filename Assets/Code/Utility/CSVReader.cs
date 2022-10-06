using System;
using System.Collections.Generic;

namespace UnitySystemFramework.Utility
{
	public class CSVReader
	{
		public int Position { get; private set; }

		public int Columns { get; private set; }

		public string Input { get; private set; }

		public int LineNumber { get; private set; }

		public int Column { get; private set; }

		public bool HasNext => Position < Input.Length;

		public CSVReader(string input)
		{
			Input = input;

			Columns = ReadFields().Length;
			Reset();
		}

		public string[] ReadFields()
		{
			var fields = new List<string>();
			while (Position < Input.Length)
			{
				if (Input[Position] == '\n')
				{
					if (Input[Position - 1] == ',')
					{
						fields.Add("");
					}
					Position++;
					break;
				}
				fields.Add(ReadField());
			}

			return fields.ToArray();
		}

		public string ReadField()
		{
			bool quote = false;
			bool escape = false;
			int startIndex = Position;
			while (Position < Input.Length)
			{
				var chr = Input[Position];

				Column++;
				if (!escape && !quote && chr == '\n')
				{
					LineNumber++;
					Column = 0;
					return Input.Substring(startIndex, Position - startIndex);
				}
				if (!escape && chr == '"')
				{
					if (quote)
					{
						if (chr == '"')
						{
							var result = Input.Substring(startIndex, Position - startIndex);
							if (Position + 1 < Input.Length && Input[Position + 1] == ',')
								Position += 2;
							return result;
						}
					}
					else
					{
						startIndex++;
						quote = true;
					}
				}
				if (!quote && !escape && chr == ',')
				{
					var result = Input.Substring(startIndex, Position - startIndex);
					Position++;
					return result;
				}
				if (!escape && chr == '\\')
				{
					escape = true;
				}
				else
				{
					escape = false;
				}

				Position++;
			}

			throw new Exception("Unable to find the pair for the quote at ");
		}

		public void Reset()
		{
			LineNumber = 0;
			Column = 0;
			Position = 0;
		}
	}
}
