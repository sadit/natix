using System;
using System.Collections.Generic;
using natix;
using natix.SimilaritySearch;
using natix.SortingSearching;

namespace HSP_app
{
	public class HS
	{
		public string name="";
		public MetricDB space;
		//
		public IList<IList<int>> Edges;
		public int max_out_degree=0;
		public int min_out_degree=0;
		public int sum_out_degree=0;


		public HS (MetricDB _space)
		{
			this.name="HS";
			this.space=_space;		
		}

		public float average_out_degree
		{
			get{
				return (float)this.sum_out_degree/(float)this.Edges.Count;
			}
		}

		public void Build(double radius=0,int big=0)
		{
			int n=this.space.Count;
			this.Edges=new IList<int>[n];
			for (int i=0;i<n;i++)
			{
				this.Edges[i]=new List<int>();
				this.Edges[i]=ComputeEdges(i,radius,big);

				this.sum_out_degree+=this.Edges[i].Count;
				if (this.Edges[i].Count > this.max_out_degree)
					this.max_out_degree=this.Edges[i].Count;
				if (this.Edges[i].Count < this.min_out_degree || this.min_out_degree==0)
					this.min_out_degree=this.Edges[i].Count;
			}
		}
		public virtual IList<int> ComputeEdges(int obj_id,double radius=0,int big=0)
		{
			return null;
		}

		// Returns the outdegree.
		public int outdegree(int obj)
		{
			return this.Edges[obj].Count;
		}

		// Returns a string with the format:
		/*
		public string GraphString()
		{
			string s="";

			for (int i=0;i<this.Edges.Count;i++)
			{
				for (int j=0;j<this.Edges[i].Count;j++)			
				{
					s+=Point2String.GetVectorsString; this.space[i]+"-"+(Edges[i][j]+1).ToString()+",";
				}
			}
			return s;
		}*/
		
		// Returns the neighbors of a point.
		public IList<int> GetNeighbors(int id)
		{
			return this.Edges[id];
		}
	}

	// ############### HSP (Half Space Proximal) ###################

	public class HSP:HS
	{

		public static int Comp_Func(double a, double b)
		{
			return a.CompareTo(b);
		}
		
		public static bool Can_I_Stop(double[] Distances,Set Candidates,double Radius)
		{
			return Radius!=0 && Distances[Candidates.First] > Radius ; 
		}

		// Tells whether we consider actual edge much bigger than the previous.
		public static bool Is_Big(double Actual_Distance,double Previous_Distance,int big)
		{
			return big != 0 && Previous_Distance!=0 && Actual_Distance/Previous_Distance>big;
		}

		public HSP (MetricDB _space): base(_space)
		{
			this.name="HSP";
		}
	

		// Computes the edges of an object
		public override IList<int> ComputeEdges(int obj_id,double radius=0,int big=0)
		{
			return ComputeEdges(this.space[obj_id],this.space,radius:radius,big:big);
			/*
			IList<int> edges=new List<int>();
			Set Candidates=new Set(this.space.Count);
			double dist_last_neig=0;


			double[] distances=new double[this.space.Count];
			int[] objs=new int[this.space.Count]; // <-- used in the sorting
			for(int j=0;j<this.space.Count;j++)
			{
				distances[j]=this.space.Dist(this.space[obj_id],this.space[j]);
				objs[j]=j;
			}
			Sorting.Sort((IList<double>)distances,(IList<int>)objs,(Comparison<double>)Comp_Func);
			
			

			while (Candidates.Cardinality>0 && distances[Candidates.First]==0)
				Candidates.Remove(Candidates.First);

			while (Candidates.Cardinality>0 && distances[Candidates.Last]==0)
				Candidates.Remove(Candidates.Last);

			int outdegree=0;

			while (Candidates.Cardinality>0 )
			{
				//Console.WriteLine("Candidates:{0}",Candidates.Cardinality);

				// get closest element	
				int closest_id=objs[Candidates.First];
				if (Can_I_Stop (distances,Candidates,radius) )
				{
					break;
				}

				if ( Is_Big(distances[Candidates.First],dist_last_neig,big) ) // 				
				{
					break;
				}
				outdegree++;
				//num_edges++;
				dist_last_neig=distances[Candidates.First];
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

			return edges;
			*/
		}




		// Computes the edges of an object (static mode) with a subset of the Data Base
		public static IList<int> ComputeEdges(object q,MetricDB db, IList<int> subset=null,double radius=0,int big=0)
		{
			IList<int> edges=new List<int>();
			int n=0;
			if (subset==null)
				n=db.Count;
			else
				n=subset.Count;
			double dist_last_neig=0;	

			double[] distances=new double[n];
			int[] objs=new int[n]; // <-- used in the sorting
		
			
			for(int j=0;j<n;j++)
			{
				if (subset==null)
					distances[j]=db.Dist(q,db[j]);
				else
					distances[j]=db.Dist(q,db[subset[j]]);
				objs[j]=j;
			}
			Sorting.Sort(distances,objs,Comp_Func);


			Set Candidates;
			Candidates=new Set(n);
			/*subset=null;
			if (subset==null) Candidates=new Set(db.Count);
			else {
				bool[] subset2=new bool[db.Count];
				for (int i=0;i<subset2.Length;i++)
					subset2[i]=subset[objs[i]];
				Candidates=new Set(db.Count,subset2);
			}*/
			while (Candidates.Cardinality>0 && distances[Candidates.First]<=0 )
				Candidates.Remove(Candidates.First);



			int outdegree=0;

			while (Candidates.Cardinality>0)
			{
				//Console.WriteLine("Candidates:{0}",Candidates.Cardinality);

				// get closest element	
				int closest_id=objs[Candidates.First];
				if (Can_I_Stop(distances,Candidates,radius))
					break;

				if ( Is_Big(distances[Candidates.First],dist_last_neig,big) ) // 				
				{
					break;
				}

				outdegree++;
				dist_last_neig=distances[Candidates.First];
				Candidates.actual=Candidates.First;
				if (subset == null)
					edges.Add(closest_id);
				else
					edges.Add(subset[closest_id]);

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

			return edges;
		}
	

	}

	// ###################### HSP no sorting construction ###############
	public class HSPnoSorting:HS
	{
		public HSPnoSorting(MetricDB _space):base(_space)
		{
			this.name="HSPnoSorting";
		}

		// Computes the edges of an object
		public override IList<int> ComputeEdges(int obj_id,double radius=0,int big=0)
		{
			return ComputeEdges(this.space[obj_id],this.space,radius:radius,big:big);
		}

		// Computes the edges of an object (static mode) with a subset of the Data Base
		public static IList<int> ComputeEdges(object q,MetricDB db, IList<int> subset=null,double radius=0,int big=0)
		{
			IList<int> edges=new List<int>();
			int closest_id=-1;

			int n=0;
			if (subset==null)
				n=db.Count;
			else
				n=subset.Count;

			double dist_last_neig=-1;	

			double[] distances=new double[n];
			//int[] objs=new int[n]; // <-- used in the sorting
		
			
			for(int j=0;j<n;j++)
			{
				if (subset==null)
					distances[j]=db.Dist(q,db[j]);
				else
					distances[j]=db.Dist(q,db[subset[j]]);

				if (closest_id==-1 && distances[j]!=0)
					closest_id=j;
				if (distances[j]!=0 && closest_id!=-1 && distances[j] < distances[closest_id])
					closest_id=j;
			}

			//Sorting.Sort(distances,objs,HSP.Comp_Func);


			Set Candidates;
			Candidates=new Set(n);
			/*subset=null;
			if (subset==null) Candidates=new Set(db.Count);
			else {
				bool[] subset2=new bool[db.Count];
				for (int i=0;i<subset2.Length;i++)
					subset2[i]=subset[objs[i]];
				Candidates=new Set(db.Count,subset2);
			}*/
			//while (Candidates.Cardinality>0 && distances[Candidates.First]<=0 )
			//	Candidates.Remove(Candidates.First);



			int outdegree=0;


			int next_close=-1;




			while (Candidates.Cardinality>0)
			{
				double dist_q_min=double.MaxValue;
				//Console.WriteLine("Candidates:{0}",Candidates.Cardinality);

				if (radius!=0 && distances[closest_id] > radius)
					break;
				if (big!=0 && dist_last_neig!=-1 && dist_last_neig / distances[closest_id] > big)
					break;


				outdegree++;
				dist_last_neig=distances[closest_id];

				Candidates.actual=Candidates.First;

				if (subset != null)
					closest_id=(subset[closest_id]);
				edges.Add(closest_id);

				// remove elements in the forbidden area
				while(Candidates.actual != -1)
				{
					double d_q_actual=0;
					double d_neig_actual=0;
					int can_act_id=Candidates.actual;
					if (subset!=null)
					{
						can_act_id=subset[Candidates.actual];
					}
					d_q_actual    = distances[Candidates.actual];
					d_neig_actual = db.Dist(db[closest_id],db[can_act_id]);



					// ***Remove points***
					if ( d_q_actual > d_neig_actual || d_q_actual==0 )
					{	
						Candidates.Remove(Candidates.actual);
						continue;
						//Console.WriteLine("Removed");
					}
				
					if (d_q_actual<dist_q_min)
					{
						next_close=Candidates.actual;	
						dist_q_min=d_q_actual;
					}

					Candidates.Next();
				}
				closest_id=next_close;


			}

			return edges;
		}
	}

	// ###################### HSP 2D Boost construction ####################
	public class HSP2D:HS 
	{
		public new MemMinkowskiVectorDB<double> space;

		public HSP2D (MemMinkowskiVectorDB<double> _space):base(_space) 
		{
			this.space=_space;
			this.name="HSP2D";
		}
		// Computes the edges of an object
		public override IList<int> ComputeEdges(int obj_id,double radius=0,int big=0)
		{
			return ComputeEdges(this.space.VECTORS[obj_id],this.space,radius:radius,big:big);
		}

		public static IList<int> ComputeEdges(double[] q, MemMinkowskiVectorDB<double> db, IList<int> subset=null,double radius=0,int big=0)
		{

			if (db.Dimension!=2)
				throw new System.InvalidOperationException("DB must be dimension 2.");

			IList<int> edges=new List<int>();
			int n=0;
			if (subset==null)
				n=db.Count;
			else
				n=subset.Count;

			double dist_last_neig=0;	

			double[] distances=new double[n];
			int[] objs=new int[n]; // <-- used in the sorting
		
			double[] Thetas=new double[n];
			int[] objsT=new int[n]; // <-- used in the sorting
			
			// fill the arrays
			for(int j=0;j<n;j++)
			{
				if (subset==null)
				{
					distances[j]=db.Dist(q,db[j]);
				}
				else
				{
					distances[j]=db.Dist(q,db[subset[j]]);
				}

				objs[j]=j;
				objsT[j]=j;
			}
			Sorting.Sort(distances,objs,HSP.Comp_Func);

			// sort by angle arround q
			for (int j=0;j<n;j++)
			{
				if (subset==null)
					Thetas[j]=Math.Atan2((double)db.VECTORS[objs[j]][1]-q[1],(double)db.VECTORS[objs[j]][0]-q[0]);
				else
					Thetas[j]=Math.Atan2((double)db.VECTORS[objs[subset[j]]][1]-q[1],(double)db.VECTORS[objs[subset[j]]][0]-q[0]);

				if (Thetas[j]<0)
						Thetas[j]+=Math.PI*2;
			}


			//Sorting.Sort(Thetas,objsT);
			Array.Sort(Thetas,objsT);


			Set Candidates;
			Candidates=new Set(n);

			while (Candidates.Cardinality>0 && distances[Candidates.First]<=0 )
				Candidates.Remove(Candidates.First);


			int outdegree=0;

			while (Candidates.Cardinality>0)
			{
				//Console.WriteLine("Candidates:{0}",Candidates.Cardinality);

				// get closest element	
				int closest_id=objs[Candidates.First];
				if (HSP.Can_I_Stop(distances,Candidates,radius))
					break;

				if ( HSP.Is_Big(distances[Candidates.First],dist_last_neig,big) ) // 				
				{
					break;
				}

				outdegree++;
				dist_last_neig=distances[Candidates.First];
				Candidates.actual=Candidates.First;
				if (subset == null)
					edges.Add(closest_id);
				else
					edges.Add(subset[closest_id]);

				// remove elements in the forbidden area
				double[] vec = new double[2] {db.VECTORS[edges[edges.Count-1]][0]-q[0],db.VECTORS[edges[edges.Count-1]][1]-q[1]};
				double min_angle=Math.Atan2(vec[1],vec[0]);
				if (min_angle<0)
					min_angle+=Math.PI*2;

				min_angle=(min_angle-Math.PI/2);
				if (min_angle<0) min_angle+=Math.PI*2;
				double max_angle=(min_angle+Math.PI)%(Math.PI*2);

				int ini_pos=Search.FindFirst(min_angle,Thetas);
				if (ini_pos==-1) ini_pos=0;
				int end_pos=Search.FindLast(max_angle,Thetas);
				//ini_pos=0;end_pos=n-1;
				int m=(end_pos-ini_pos>0)? end_pos-ini_pos+1:n-Math.Abs(end_pos-ini_pos)+1;

				//Console.WriteLine("Near:{2} I:{0} F:{1}",min_angle,max_angle,Candidates.First);

				Set Candidates_to_check=new Set(m);
				//Console.WriteLine("{0} vs {1}",m,Candidates.Cardinality);
				while(Candidates_to_check.actual != -1)
				{

					int act_elem=(ini_pos+Candidates_to_check.actual)%n;
					if ( Candidates.elements[objsT[act_elem]]==true )
					{					
						//Console.Write("{0} ",objsT[act_elem]);
						//if (db.Dist(db[closest_id],db[objs[objsT[act_elem]]])==0)
						//	Console.WriteLine("GGGGGGGGGGGGGGGGGGGood");
						if ( distances[objsT[act_elem]] > db.Dist(db[closest_id],db[objs[objsT[act_elem]]]))
						{	
							Candidates_to_check.Remove(Candidates_to_check.actual);
							Candidates.Remove(objsT[act_elem]);
							//Console.Write("[R] ");
							continue;

						}
					}
					Candidates_to_check.Next();
				}
				//Candidates.Next();

			}

			return edges;
		}
	}

	// ###################### HSD (Half Space Distant) #####################

	public class HSD:HS
	{	

		private static int Comp_Func(double a, double b)
		{
			return -(a.CompareTo(b));
		}
		
		private static bool Can_I_Stop(double[] Distances,Set Candidates,double Radius)
		{
			return false;
			//return Radius!=0 && Distances[Candidates.First] > Radius ; 
		}

		// Tells whether we consider actual edge much bigger than the previous.
		private static bool Is_Big(double Actual_Distance,double Previous_Distance,int big)
		{
			return false;
			//return big != 0 && Previous_Distance!=0 && Actual_Distance/Previous_Distance>big;
		}

		public HSD (MetricDB _space):base(_space)
		{
			this.name="HSD";
		}

	

		// Computes the edges of an object
		public override IList<int> ComputeEdges(int obj_id,double radius=0,int big=0)
		{
			return ComputeEdges(this.space[obj_id],this.space,radius:radius,big:big);
			/*
			IList<int> edges=new List<int>();
			Set Candidates=new Set(this.space.Count);
			double dist_last_neig=0;


			double[] distances=new double[this.space.Count];
			int[] objs=new int[this.space.Count]; // <-- used in the sorting
			for(int j=0;j<this.space.Count;j++)
			{
				distances[j]=this.space.Dist(this.space[obj_id],this.space[j]);
				objs[j]=j;
			}
			Sorting.Sort((IList<double>)distances,(IList<int>)objs,(Comparison<double>)Comp_Func);
			
			

			while (Candidates.Cardinality>0 && distances[Candidates.First]==0)
				Candidates.Remove(Candidates.First);

			while (Candidates.Cardinality>0 && distances[Candidates.Last]==0)
				Candidates.Remove(Candidates.Last);

			int outdegree=0;

			while (Candidates.Cardinality>0 )
			{
				//Console.WriteLine("Candidates:{0}",Candidates.Cardinality);

				// get closest element	
				int closest_id=objs[Candidates.First];
				if (Can_I_Stop (distances,Candidates,radius) )
				{
					break;
				}

				if ( Is_Big(distances[Candidates.First],dist_last_neig,big) ) // 				
				{
					break;
				}
				outdegree++;
				//num_edges++;
				dist_last_neig=distances[Candidates.First];
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

			return edges;
			*/
		}




		// Computes the edges of an object (static mode) with a subset of the Data Base
		public static IList<int> ComputeEdges(object q,MetricDB db, IList<int> subset=null,double radius=0,int big=0)
		{
			IList<int> edges=new List<int>();
			int n=0;
			if (subset==null)
				n=db.Count;
			else
				n=subset.Count;
			double dist_last_neig=0;	

			double[] distances=new double[n];
			int[] objs=new int[n]; // <-- used in the sorting
		
			
			for(int j=0;j<n;j++)
			{
				if (subset==null)
					distances[j]=db.Dist(q,db[j]);
				else
					distances[j]=db.Dist(q,db[subset[j]]);
				objs[j]=j;
			}
			Sorting.Sort(distances,objs,Comp_Func);


			Set Candidates;
			Candidates=new Set(n);

			while (Candidates.Cardinality>0 && distances[Candidates.Last]<=0 )
				Candidates.Remove(Candidates.Last);

			int outdegree=0;

			while (Candidates.Cardinality>0)
			{
				//Console.WriteLine("Candidates:{0}",Candidates.Cardinality);

				// get closest element	
				int closest_id=objs[Candidates.First];
				if (Can_I_Stop(distances,Candidates,radius))
					break;

				if ( Is_Big(distances[Candidates.First],dist_last_neig,big) ) // 				
				{
					break;
				}

				outdegree++;
				dist_last_neig=distances[Candidates.First];
				Candidates.actual=Candidates.First;
				if (subset == null)
					edges.Add(closest_id);
				else
					edges.Add(subset[closest_id]);

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

			return edges;
		}


	}

	// ###################### HSR (Half Space Random) #####################

	public class HSR:HS
	{	

		private static void Shuffle<KeyType,ValueType>(KeyType[] Keys,ValueType[] Values)
		{
			Random rnd=new Random();

			for (int i=Keys.Length-1;i>=0;i--)
			{
				int k=rnd.Next(i+1);
				var temp=Keys[i];
				Keys[i]=Keys[k];
				Keys[k]=temp;
				var temp2=Values[i];
				Values[i]=Values[k];
				Values[k]=temp2;
			}
		}
		private static int Comp_Func(int a, int b)
		{
			return (a.CompareTo(b));
		}
		
		private static bool Can_I_Stop(double[] Distances,Set Candidates,double Radius)
		{
			return false;
			//return Radius!=0 && Distances[Candidates.First] > Radius ; 
		}

		// Tells whether we consider actual edge much bigger than the previous.
		private static bool Is_Big(double Actual_Distance,double Previous_Distance,int big)
		{
			return false;
			//return big != 0 && Previous_Distance!=0 && Actual_Distance/Previous_Distance>big;
		}

		public HSR (MetricDB _space):base(_space)
		{
			this.name="HSR";
		}

	

		// Computes the edges of an object
		public override IList<int> ComputeEdges(int obj_id,double radius=0,int big=0)
		{
			return ComputeEdges(this.space[obj_id],this.space,radius:radius,big:big);
		}




		// Computes the edges of an object (static mode) with a subset of the Data Base
		public static IList<int> ComputeEdges(object q,MetricDB db, IList<int> subset=null,double radius=0,int big=0)
		{
			IList<int> edges=new List<int>();
			int n=0;
			if (subset==null)
				n=db.Count;
			else
				n=subset.Count;
			double dist_last_neig=0;	

			double[] distances=new double[n];
			int[] objs=new int[n]; // <-- used in the sorting
		
			
			for(int j=0;j<n;j++)
			{
				if (subset==null)
					distances[j]=db.Dist(q,db[j]);
				else
					distances[j]=db.Dist(q,db[subset[j]]);
				objs[j]=j;
			}	
			Shuffle(distances,objs);


			Set Candidates;
			Candidates=new Set(n);

			//while (Candidates.Cardinality>0 && distances[Candidates.Last]<=0 )
			//	Candidates.Remove(Candidates.Last);

			int outdegree=0;

			while (Candidates.Cardinality>0)
			{
				//Console.WriteLine("Candidates:{0}",Candidates.Cardinality);

				// get closest element	
				int closest_id;
				do {
					closest_id=objs[Candidates.First];
					if (distances[closest_id]!=0) 
						break;
					else
					{
						Candidates.Remove(Candidates.First);
					}
				}while(Candidates.Cardinality>0 );

				if (distances[closest_id]==0)
					break;

				if (Can_I_Stop(distances,Candidates,radius))
					break;

				if ( Is_Big(distances[Candidates.First],dist_last_neig,big) ) // 				
				{
					break;
				}

				outdegree++;
				dist_last_neig=distances[Candidates.First];
				Candidates.actual=Candidates.First;
				if (subset == null)
					edges.Add(closest_id);
				else
					edges.Add(subset[closest_id]);


				// remove elements in the forbidden area
				while(Candidates.actual != -1)
				{

					if (distances[Candidates.actual]==0 || distances[Candidates.actual] > db.Dist(db[closest_id],db[objs[Candidates.actual]]))
					{	
						Candidates.Remove(Candidates.actual);
						continue;
						//Console.WriteLine("Removed");
					}


					Candidates.Next();					

				}

			}

			return edges;
		}


	}
	// ################################## S E T ##########################

	public class Set
	{
		public bool[] elements;
		public int Cardinality=0;
		public int First=0;
		public int Last=0;
		public int actual;
		public int size;
		public Set(int n)
		{
			this.elements=new bool[n];
			this.Cardinality=n;
			this.First=0;
			this.Last=n-1;
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
			this.Last=n-1;
			GetLast();
			GetFirst();
			this.actual=this.First;
		}
		public void Remove(int id)
		{
			if (this.elements[id]==true)
			{
				this.elements[id]=false;
				if (id==this.First)
				{
					GetFirst();
				}
				if ( id == this.actual)
					Next();
				if ( id == this.Last )
					GetLast();
				this.Cardinality--;
			}
		}
		public void GetFirst()
		{
			if (this.First!=-1)
			{
				for (int i=this.First;i<=this.Last;i++)
				{
					if (elements[i]==true)
					{	this.First=i;
						return;
					}
				}
				this.First=-1;
			}
		}
		public void GetLast()
		{
			if (this.Last!=-1 && this.First!=-1)
			{
				for (int i=this.Last;i>=this.First;i--)
				{
					if (elements[i]==true)
					{	this.Last=i;
						return;
					}
				}
				this.Last=-1;
			}
		}
		// points this.actual to the next 
		public int Next()
		{
			if (this.actual != -1 && this.actual!=this.Last )
				for (int i=this.actual+1;i<=this.Last;i++)
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
		
	public class Point2String
	{
		// Returns the point as a string
		public static string GetPointString(IList<double> vector,IList<double> respectTo = null)
		{
			string s="";
			for (int i=0;i<vector.Count;i++)
			{
				if (respectTo==null)
					s+=vector[i]+" ";
				else
					s+=(vector[i]-respectTo[i]).ToString()+" ";
			}
			return s;
		}
		//Returns a string with the vectors (edges) of a point in the graph.
		public static string GetVectorsString(int id,IList<IList<double>> vectors,IList<IList<int>> edges)
		{
			string s="";
			if (edges[id].Count==0) return " ";
			s+=GetPointString(vectors[id]) + " " + GetPointString(vectors[edges[id][0]],vectors[id]) ;
			for (int j=1;j<edges[id].Count;j++)			
			{
				s+=Environment.NewLine;
				s+=GetPointString(vectors[id]) + " " + GetPointString(vectors[edges[id][j]],vectors[id]) ;
			}
			return s;
		}
		//Returns a string with the vectors of the graph
		public static string GraphVectorsString(IList<IList<double>> vectors,IList<IList<int>> edges)
		{
			string s="";
			for (int i=0;i<edges.Count;i++)
			{
				for (int j=0;j<edges[i].Count;j++)			
				{
					s+=GetPointString(vectors[i]) + " " + GetPointString(vectors[edges[i][j]],vectors[i]) ;
					s+=Environment.NewLine;
				}
			}
			return s;
		}
	}
}