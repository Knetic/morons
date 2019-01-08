using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace Gridsnap.Morons
{
	[Serializable]
	public class StatemapDefinition
	{
		[SerializeField, HideInInspector]
		public List<StatemapField> definitions;

		public StatemapDefinition()
		{
			definitions = new List<StatemapField>();
		}

		public IEnumerable<StatemapField> getDefinitions()
		{
			return definitions;
		}

		public System.Object get(String key)
		{
			return
				definitions
				.Where(d => d.name == key)
				.Select(d => d.value)
				.FirstOrDefault();
		}

		public void set(String key, Type t, System.Object value)
		{
			StatemapField field;

			field =
				definitions
				.Where(d => d.name == key)
				.FirstOrDefault();
			
			if(field == null)
			{
				field = new StatemapField();
				field.name = key;
				definitions.Add(field);
			}
			
			field.value = value;
		}

		public IEnumerable<StatemapField> getFieldsForType(Type t)
		{
			return
				definitions
				.Where(d => d.getType() == t);
		}
	}
}