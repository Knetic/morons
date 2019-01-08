using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace Gridsnap.Morons
{
	/// <summary>
	///		A node that can be connected to other nodes, each indicating something the Moron should do for one or more frames.
	/// </summary>
	[Serializable]
	public abstract class MoronIONode : GraphNode
	{
		[NonSerialized]
		public MoronThinker thinker;

		[NonSerialized]
		public String guid = System.Guid.NewGuid().ToString();

		/// <summary>
		///		If true, this node will be evaluated even if the current Intent isn't finished.
		///		Only has a real effect on Choices, though other types may need it in the future.
		/// </summary>
		[NonSerialized]
		public bool interruptsIntent;

		/// <summary>
		///		Joists map an IONode field name to a designer-defined statemap variable name.
		///		Keys are the field name in the implementing node class to be read/written.
		///		Values are the designer-defined variable name that will be used as the key to read/write the statemap during execution.
		///		Joists are not used after `init` is called.
		/// </summary>
		[NonSerialized]
		protected List<Pair<String, String>> readJoists = new List<Pair<String, String>>();
		[NonSerialized]
		protected List<Pair<String, String>> writeJoists = new List<Pair<String, String>>();

		/// <summary>
		///		A list of designer-defined key/values for manually-set fields.
		///		Each key is the field name in the implementing node class to be written.
		///		Each value is the designer-defined value for that field.
		/// </summary>
		[NonSerialized]
		protected List<Pair<String, System.Object>> manualJoists = new List<Pair<String, System.Object>>();

		/// <summary>
		///		The actual map used to get/set values in the statemap during gameplay.
		///		The key is always the name of the statemap key whose contents should be read-from/written-to the FieldInfo value given here.
		/// </summary>
		[NonSerialized]
		private Dictionary<String, FieldInfo> statemapRead, statemapWrite;

		/// <summary>
		///		Executed whatever logic needs to happen, and return the edge to the next IONode that should be activated.
		///		If null is returned (which is not unusual) then no state transition will occur.
		/// </summary>
		public abstract GraphEdge update();

		public MoronIONode() : base(Vector3.zero)
		{
			statemapRead = new Dictionary<String, FieldInfo>();
			statemapWrite = new Dictionary<String, FieldInfo>();
		}

		/// <summary>
		/// 	Called once when the node is properly initialized with all edges that it will ever have.
		/// </summary>
		public virtual void init()
		{
			Type ourType;

			ourType = GetType();

			foreach(Pair<String, String> joist in readJoists)
				statemapRead[joist.value] = ourType.GetField(joist.key);
			foreach(Pair<String, String> joist in writeJoists)
				statemapWrite[joist.value] = ourType.GetField(joist.key);

			foreach(Pair<String, System.Object> joist in manualJoists)
				ourType.GetField(joist.key).SetValue(this, joist.value);
		}

		public virtual void activate()
		{
			foreach(String key in statemapRead.Keys)
				statemapRead[key].SetValue(this, thinker.statemap[key]);
		}

		public virtual void deactivate()
		{
			foreach(String key in statemapWrite.Keys)
				thinker.statemap[key] = statemapWrite[key].GetValue(this);
		}

		public virtual void notifyNearby()
		{}
		public virtual void notifyNotNearby()
		{}

		public IEnumerable<FieldInfo> getFieldsWithAttribute<T>() where T: Attribute
		{
			return 
				GetType()
				.GetFields()
				.Where
				(
					f => 
					f.GetCustomAttributes(true)
					.Where(a => a is T)
					.Any()
				);
		}

		public void setJoistsTo(MoronIONode other)
		{
			readJoists = other.readJoists;
			writeJoists = other.writeJoists;
			manualJoists = other.manualJoists;
		}

		protected IEnumerable<T> getNeighbors<T>()
		{
			return 
				edges.Select(e => e.to)
				.Where(e => e is T)
				.Cast<T>();
		}

		public List<Pair<String, String>> getReadJoists()
		{
			return readJoists;
		}

		public List<Pair<String, String>> getWriteJoists()
		{
			return writeJoists;
		}

		public List<Pair<String, System.Object>> getManualJoists()
		{
			return manualJoists;
		}

		public String getReadJoist(String key)
		{
			return readJoists.Where(j => j.key == key).FirstOrDefault().value;
		}

		public String getWriteJoist(String key)
		{
			return writeJoists.Where(j => j.key == key).FirstOrDefault().value;
		}

		public System.Object getManualJoist(String key)
		{
			System.Object value;

			value = manualJoists.Where(j => j.key == key).FirstOrDefault().value;
			if(value != null)
				return value;

			return GetType().GetField(key).GetValue(this);
		}

		public void setReadJoist(String key, String value)
		{
			setJoist<String>(readJoists, key, value);
		}

		public void setWriteJoist(String key, String value)
		{
			setJoist<String>(writeJoists, key, value);
		}

		public void setManualJoist(String key, System.Object value)
		{
			setJoist<System.Object>(manualJoists, key, value);	
			GetType().GetField(key).SetValue(this, value);
		}

		protected void setJoist<T>(List<Pair<String, T>> joists, String key, T value)
		{
			joists.RemoveAll(j => j.key == key);
			joists.Add(new Pair<String, T>(key, value));
		}
	}
}