using System;
using System.Collections.Generic;
using natix;
using natix.SimilaritySearch;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	public class HSP
	{

		public MetricDB space;
		public IList<IList<int>> Edges;
		public int max_out_degree=0;
		public int min_out_degree=0;
		int sum_out_degree=0;
		public HSP (MetricDB _space)
		{
			this.space=_space;		
		}

		public void Build()
		{
			int n=this.space.Count;
			Edges=new IList<int>[n];
			for (int i=0;i<n;i++)
			{
				Edges[i]=new List<int>();
				Edges[i]=ComputeEdges(i);
			}
		}


		public float average_out_degree
		{
			get{
				return (float)this.sum_out_degree/(float)this.Edges.Count;
			}
		}

		// Computes the edges of an object
		public IList<int> ComputeEdges(int obj_id)
		{
			IList<int> edges=new List<int>();
			Set Candidates=new Set(this.space.Count);

			double[] distances=new double[this.space.Count];
			int[] objs=new int[this.space.Count]; // <-- used in the sorting
			for(int j=0;j<this.space.Count;j++)
			{
				distances[j]=this.space.Dist(this.space[obj_id],this.space[j]);
				objs[j]=j;
			}
			Sorting.Sort(distances,objs);

			while (Candidates.Cardinality>0 && distances[Candidates.First]==0)
				Candidates.Remove(Candidates.First);

			int outdegree=0;

			while (Candidates.Cardinality>0)
			{
				//Console.WriteLine("Candidates:{0}",Candidates.Cardinality);

				// get closest element	
				int closest_id=objs[Candidates.First];
				outdegree++;
				Candidates.actual=Candidates.First;
				edges.Add(closest_id);

				// remove elements in the forbidden area
				while(Candidates.actual != -1)
				{
					//Console.WriteLine(Candidates.actual);
					if ( distances[Candidates.actual] > space.Dist(space[closest_id],space[objs[Candidates.actual]]))
					{	
						Candidates.Remove(Candidates.actual);
						continue;
						//Console.WriteLine("Removed");
					}
					Candidates.Next();
				}

			}
			this.sum_out_degree+=edges.Count;
			if (edges.Count > this.max_out_degree)
				this.max_out_degree=edges.Count;
			if (edges.Count < this.min_out_degree || this.min_out_degree==0)
				this.min_out_degree=edges.Count;

			return edges;
		}

		// Computes the edges of an object (static mode)
		public static IList<int> ComputeEdges(object q,MetricDB db, bool[] subset=null)
		{
			IList<int> edges=new List<int>();
			Set Candidates;
			if (subset==null) Candidates=new Set(db.Count);
			else Candidates=new Set(db.Count,subset);

			double[] distances=new double[db.Count];
			int[] objs=new int[db.Count]; // <-- used in the sorting
			for(int j=0;j<db.Count;j++)
			{
				distances[j]=db.Dist(q,db[j]);
				objs[j]=j;
			}
			Sorting.Sort(distances,objs);

			while (Candidates.Cardinality>0 && distances[Candidates.First]==0 )
				Candidates.Remove(Candidates.First);

			int outdegree=0;

			while (Candidates.Cardinality>0)
			{
				//Console.WriteLine("Candidates:{0}",Candidates.Cardinality);

				// get closest element	
				int closest_id=objs[Candidates.First];
				outdegree++;
				Candidates.actual=Candidates.First;
				edges.Add(closest_id);

				// remove elements in the forbidden area
				while(Candidates.actual != -1)
				{
					//Console.WriteLine(Candidates.actual);
					if ( distances[Candidates.actual] > db.Dist(db[closest_id],db[objs[Candidates.actual]]))
					{	
						Candidates.Remove(Candidates.actual);
						continue;
						//Console.WriteLine("Removed");
					}
					Candidates.Next();
				}

			}
			//this.sum_out_degree+=edges.Count;
			//if (edges.Count > this.max_out_degree)
			//	this.max_out_degree=edges.Count;
			//if (edges.Count < this.min_out_degree || this.min_out_degree==0)
			//	this.min_out_degree=edges.Count;

			return edges;
		}

		// Returns the outdegree.
		public int outdegree(int obj)
		{
			return this.Edges[obj].Count;
			/*
			Set Candidates=new Set(this.space.Count);
			//Console.WriteLine(this.space.Count);

			double[] distances=new double[this.space.Count];
			int[] objs=new int[this.space.Count];
			for(int j=0;j<this.space.Count;j++)
			{
				distances[j]=this.space.Dist(this.space[obj],this.space[j]);
				objs[j]=j;
			}
			Sorting.Sort(distances,objs);

			while (distances[Candidates.First]==0)
				Candidates.Remove(Candidates.First);

			int outdegree=0;

			while (Candidates.Cardinality>0)
			{
				//Console.WriteLine("Candidates:{0}",Candidates.Cardinality);

				// get closest element	
				int closest_id=objs[Candidates.First];
				outdegree++;
				Candidates.actual=Candidates.First;

				// remove elements in the forbidden area
				while(Candidates.actual != -1)
				{
					//Console.WriteLine(Candidates.actual);
					if ( distances[Candidates.actual] > space.Dist(space[closest_id],space[objs[Candidates.actual]]))
					{	
						Candidates.Remove(Candidates.actual);
						continue;
						//Console.WriteLine("Removed");
					}
					Candidates.Next();
				}

			}
			return outdegree;
			*/
		}
		// Returns a string with the format:
		// n:point-neighbour,point-neighbour,point-neighbour,...
		public string GraphString()
		{
			string s="";
			s+=this.Edges.Count.ToString();
			s+=":";
			for (int i=0;i<this.Edges.Count;i++)
			{
				for (int j=0;j<this.Edges[i].Count;j++)			
				{
					s+=(i+1).ToString()+"-"+(Edges[i][j]+1).ToString()+",";
				}
			}
			return s;
		}

		// Returns the neighbors of a point.
		public IList<int> GetNeighbors(int id)
		{
			return this.Edges[id];
		}
	}

	public class Set
	{
		public bool[] elements;
		public int Cardinality=0;
		public int First=0;
		public int actual;
		public int size;
		public Set(int n)
		{
			this.elements=new bool[n];
			this.Cardinality=n;
			this.First=0;
			this.actual=0;
			this.size=n;
			for (int i=0;i<n;i++)
				this.elements[i]=true;
		}
		public Set(int n,bool[] subset)
		{
			this.elements=subset;
			for (int i=0;i<n;i++)
				if ( subset[i] ) this.Cardinality++;
			this.size=n;
			GetFirst();
			this.actual=this.First;
		}
		public void Remove(int id)
		{
			this.elements[id]=false;
			if (id==this.First)
			{
				GetFirst();
			}
			if ( id == this.actual)
				Next();
			this.Cardinality--;
		}
		public void GetFirst()
		{
			if (this.First!=-1)
			{
				for (int i=this.First;i<this.elements.Length;i++)
				{
					if (elements[i]==true)
					{	this.First=i;
						return;
					}
				}
				this.First=-1;
			}
		}
		public int Next()
		{
			if (this.actual != -1 && this.actual!=this.size )
				for (int i=this.actual+1;i<this.elements.Length;i++)
			{
				if (elements[i]==true)
				{	
					this.actual=i;
					return i;
				}
			}
			this.actual=-1;
			return -1;
		}
	}
}