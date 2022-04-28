using CourseWork.BLL.Models;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MeshImport
{
	internal static class ExcelReaderFE
	{
		static ExcelReaderFE()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}

		internal static List<Node> GetNodes(string fileName)
		{
			var stream = File.Open(fileName, FileMode.Open, FileAccess.Read);
			var reader = ExcelReaderFactory.CreateOpenXmlReader(stream, new ExcelReaderConfiguration()
			{
				FallbackEncoding = Encoding.GetEncoding(1252),
			});

			var nodes = new List<Node>();
			var startRead = true;

			do
			{
				while (reader.Read())
				{
					if (startRead)
					{
						startRead = false;
						continue;
					}

					nodes.Add(new Node { Id = Convert.ToInt32(reader.GetDouble(0)), X = reader.GetDouble(1), Y = reader.GetDouble(2)});
				}
			}
			while (reader.NextResult());

			reader.Dispose();
			stream.Dispose();

			return ReverseNodes(nodes);
		}

		internal static List<TriangularFiniteElement> GetFiniteElements(string fileName)
		{
			var stream = File.Open(fileName, FileMode.Open, FileAccess.Read);
			var reader = ExcelReaderFactory.CreateOpenXmlReader(stream, new ExcelReaderConfiguration()
			{
				FallbackEncoding = Encoding.GetEncoding(1252),
			});

			var finiteElements = new List<TriangularFiniteElement>();
			var startRead = true;

			do
			{
				while (reader.Read())
				{
					if (startRead)
					{
						startRead = false;
						continue;
					}

					var c1 = Convert.ToInt32(reader.GetDouble(0));
					var c2 = Convert.ToInt32(reader.GetDouble(2));
					var c3 = Convert.ToInt32(reader.GetDouble(3));
					var c4 = Convert.ToInt32(reader.GetDouble(4));
                    var triangularFiniteElement = new TriangularFiniteElement(new List<Node>
                    {
                        new Node { Id = c2 - 1 },
                        new Node { Id = c3 - 1 },
                        new Node { Id = c4 - 1 },
                    })
                    {
                        Id = c1
                    };
                    finiteElements.Add(triangularFiniteElement);
				}
			}
			while (reader.NextResult());

			reader.Dispose();
			stream.Dispose();

			return finiteElements;
		}

		private static List<Node> ReverseNodes(IEnumerable<Node> timeNodes)
		{
			var nodes = new List<Node>();

			var maxY = timeNodes.Select(timeNode => timeNode.Y).Max();

			foreach (var timeNode in timeNodes)
			{
				nodes.Add(new Node(timeNode.Id - 1, timeNode.X, maxY - timeNode.Y + 200));
			}

			return nodes;
		}
	}
}
