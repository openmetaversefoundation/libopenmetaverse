using System;
using System.Collections;

namespace SecondSuite.Plugins
{
	public class PluginCollection : CollectionBase
	{
		public PluginCollection() 
		{
		}

		public PluginCollection(PluginCollection value)	
		{
			this.AddRange(value);
		}

		public PluginCollection(SSPlugin[] value)
		{
			this.AddRange(value);
		}

		public SSPlugin this[int index] 
		{
			get	{return ((SSPlugin)(this.List[index]));}
		}

		public int Add(SSPlugin value) 
		{
			return this.List.Add(value);
		}

		public void AddRange(SSPlugin[] value) 
		{
			for (int i = 0;	(i < value.Length); i = (i + 1)) 
			{
				this.Add(value[i]);
			}
		}

		public void AddRange(PluginCollection value) 
		{
			for (int i = 0;	(i < value.Count); i = (i +	1))	
			{
				this.Add((SSPlugin)value.List[i]);
			}
		}

		public bool Contains(SSPlugin value) 
		{
			return this.List.Contains(value);
		}

		public void CopyTo(SSPlugin[] array, int index) 
		{
			this.List.CopyTo(array, index);
		}

		public SSPlugin[] ToArray()
		{
			SSPlugin[] array = new SSPlugin[this.Count];
			this.CopyTo(array, 0);
			
			return array;
		}

		public int IndexOf(SSPlugin value) 
		{
			return this.List.IndexOf(value);
		}

		public void Insert(int index, SSPlugin value)	
		{
			List.Insert(index, value);
		}
		
		public void Remove(SSPlugin value) 
		{
			List.Remove(value);
		}

		public new PluginCollectionEnumerator GetEnumerator()	
		{
			return new PluginCollectionEnumerator(this);
		}

		public class PluginCollectionEnumerator : IEnumerator	
		{
			private	IEnumerator _enumerator;
			private	IEnumerable _temp;
			
			/// <summary>
			/// Initializes a new instance of the <see cref="PluginCollectionEnumerator">PluginCollectionEnumerator</see> class referencing the specified <see cref="PluginCollection">PluginCollection</see> object.
			/// </summary>
			/// <param name="mappings">The <see cref="PluginCollection">PluginCollection</see> to enumerate.</param>
			public PluginCollectionEnumerator(PluginCollection mappings)
			{
				_temp =	((IEnumerable)(mappings));
				_enumerator = _temp.GetEnumerator();
			}
			
			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public SSPlugin Current
			{
				get {return ((SSPlugin)(_enumerator.Current));}
			}
			
			object IEnumerator.Current
			{
				get {return _enumerator.Current;}
			}
			
			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns><b>true</b> if the enumerator was successfully advanced to the next element; <b>false</b> if the enumerator has passed the end of the collection.</returns>
			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}
			
			bool IEnumerator.MoveNext()
			{
				return _enumerator.MoveNext();
			}
			
			/// <summary>
			/// Sets the enumerator to its initial position, which is before the first element in the collection.
			/// </summary>
			public void Reset()
			{
				_enumerator.Reset();
			}
			
			void IEnumerator.Reset() 
			{
				_enumerator.Reset();
			}
		}
	}
}
