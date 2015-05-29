//
//  Copyright 2014  Luis Guillermo Ruiz Velázquez
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Collections.Generic;
using natix;
using natix.SimilaritySearch;
using System.IO;

namespace VPForest
{
	internal interface VPF_Vertex
	{
		void Save (BinaryWriter output);
		void Load (BinaryReader input);
	}
	internal class VPF_Node:VPF_Vertex
	{
		public double Lmin;
		public double Lmax;
		public double Rmin;
		public double Rmax;
		internal VPF_Vertex left;
		internal VPF_Vertex right;
		public int center;

		public VPF_Node (){
		}

		public VPF_Node(int _center)
		{
			this.center=_center;
		}
		public void Save(BinaryWriter output)
		{
			output.Write (Lmin);
			output.Write (Lmax);
			output.Write (Rmin);
			output.Write (Rmax);
			output.Write (center);


			if (this.left is VPF_Node)
				output.Write (true);
			else  
				output.Write (false);
			this.left.Save (output);

			if (this.right is VPF_Node)
				output.Write (true);
			else 
				output.Write (false);
			this.right.Save (output);
		}
		public void Load (BinaryReader input)
		{
			this.Lmin=input.ReadDouble ();
			this.Lmax=input.ReadDouble ();
			this.Rmin=input.ReadDouble ();
			this.Rmax=input.ReadDouble ();
			this.center=input.ReadInt32 ();

			bool isnode = input.ReadBoolean ();
			if (isnode == true)
				this.left = new VPF_Node ();
			else 
				this.left = new VPF_Leaf ();
			this.left.Load (input); 

			isnode= input.ReadBoolean ();
			if (isnode == true)
				this.right = new VPF_Node ();
			else 
				this.right = new VPF_Leaf ();
			this.right.Load (input);
		}
		/*public VPF_Node(double _Lmin, double _Rmax)
		{
			this.Lmin=_Lmin;
			this.Rmax=_Rmax;
		}*/
	}
	internal class VPF_Leaf:VPF_Vertex
	{
		//public int Elem_id;
		public int[] IDs;
		public double[] dist2center;
		public int center;
		/*public VPF_Leaf(int elem_id,double dist_2_center)
		{
			this.IDs=new int[1]{elem_id};
			this.dist2center=new double[1]{dist_2_center};
		}*/
		public VPF_Leaf()
		{
		}
		public VPF_Leaf(int count)
		{
			this.IDs=new int[count];
			this.dist2center=new double[count];
		}
		public void SetValues(int elem_id,double dist_2_center,int index)
		{
			this.IDs[index]=elem_id;
			this.dist2center[index]=dist_2_center;
		}
		public void Save(BinaryWriter output)
		{
			output.Write (this.IDs.Length);
			PrimitiveIO<int>.SaveVector (output, this.IDs);
			output.Write (this.dist2center.Length);
			PrimitiveIO<double>.SaveVector (output, this.dist2center);
			output.Write (center);
		}
		public void Load(BinaryReader input)
		{
			int size = input.ReadInt32 ();
			this.IDs = new int[size];
			PrimitiveIO<int>.LoadVector (input,size, this.IDs);
			size = input.ReadInt32 ();
			this.dist2center = new  double[size];
			PrimitiveIO<double>.LoadVector (input,size, this.dist2center);
			this.center=input.ReadInt32 ();
		}

	}

	internal class VPF_Tree
	{
		internal VPF_Vertex Root;


		public VPF_Tree()
		{
		}

		public void Save(BinaryWriter bw)
		{
			if (this.Root is VPF_Node)
				bw.Write (true);
			else if (this.Root is VPF_Leaf)
				bw.Write (false);
			this.Root.Save (bw);
		}
		public void Load(BinaryReader br)
		{
			bool isnode = br.ReadBoolean ();
			if (isnode == true)
				this.Root = new VPF_Node ();
			else if (isnode == false)
				this.Root = new VPF_Leaf ();
			this.Root.Load (br);

		}

	}

	public class VP_Forest:BasicIndex
	{
		//MetricDB DB;	
		double m; // used in construction
		List<VPF_Tree> Forest;
		List<int> Remaining; // used in construction
		bool[] Persist; // used in construction
		//int center_id;
		//int[] perm;
		public double Tau;
		double wanted_Tau; // used in construction
		bool BuildingWithTau=false; // used in construction
		int leafs=0;

		public VP_Forest ()
		{
		}

		// Constructor
		// _m must be a value in [0,1]
		public VP_Forest (MetricDB _DB, double _m=0,double _tau=0)
		{
			this.DB=_DB;


			this.Remaining=new List<int>(this.DB.Count);
			this.Persist=new bool[this.DB.Count];
			this.Tau=double.MaxValue;
			int[] perm;
			if (_m!=0)
				this.m=_m;
			if (_tau!=0)
			{
				BuildingWithTau=true;
				this.wanted_Tau=2*_tau;
			}
			
			this.Forest=new List<VPF_Tree>();

			int N=this.DB.Count;	

			for (int i=0;i<N;i++)
				this.Remaining.Add(i);

			do
			{
				int a,w,center;
				double[] Distances=new double[N];
				this.Persist=new bool[N];
				this.Forest.Add ( new VPF_Tree() );
				perm=new int[N];
				for (int i=0;i<N;i++) {perm[i]=i;}

				//Console.WriteLine("Building tree number: {0}",Forest.Count);
				//Console.WriteLine("Remaining: {0}",Remaining.Count);
				if (Divide_Interval(N,out a,out w,out center,out Distances,perm))
				{
					this.Forest[this.Forest.Count-1].Root=new VPF_Node(center);
					BuildTree((VPF_Node)this.Forest[this.Forest.Count-1].Root,Distances,perm,a,w);
				}
				else // The root is a leaf
				{
					leafs+=N;
					VPF_Leaf L=new VPF_Leaf(N);
					L.center=center;
					for (int i=0;i<N;i++)
						L.SetValues(this.Remaining[perm[i]],Distances[i],i);
					this.Forest[this.Forest.Count-1].Root=L;
					Console.WriteLine("Tau very big. Remaining:{0}",Remaining.Count);
				}


				RemoveMarkedOnes();
				N=Remaining.Count;	

			}while(Remaining.Count>0);


			this.Tau/=2;


			Console.WriteLine("Total trees: {0}",this.Forest.Count);
			Console.WriteLine("Tau: {0}",this.Tau);
			Console.WriteLine("Leafs:{0}",leafs);

		}

		// Build the Tree!!
		internal void BuildTree(VPF_Node Node,double[] Distances,int [] Perm,int a,int w)
		{
			//int start=Node.start;
			//int end=Node.end;
			int a2=0,w2=0,center2=0;
			int count=Perm.Length;
			//Console.WriteLine("a:{0},w:{1},total:{2}",a,w,count);
			double lmin=Distances[0],lmax=Distances[a-1],rmin=Distances[a+w],rmax=Distances[Perm.Length-1];
			//double[] Distances=new double[Perm.Length];
			//int center=0;
			//Divide_Interval(count,out a,out w,Node,Distances,Perm);


			// Mark to Remove middle part
			if (w>0)
			{
				Mark2PersistRange(a,w,Perm);
				if (Distances[ a+w-1 ]-Distances[ a ]>0) 
					this.Tau=Math.Min(this.Tau,Distances[ a+w-1 ]-Distances[ a ]);
			}

			double[] Distances2=new double[a];
			int[] Perm2=new int[a];
			for (int i=0;i<a;i++)
			{	Perm2[i]=Perm[i];
				//Distances2[i]=Distances[i];
			}
			if (Divide_Interval(a,out a2,out w2,out center2,out Distances2,Perm2)) // build left node			
			{
				Node.Lmin=lmin;
				Node.Lmax=lmax;
				Node.left=new VPF_Node( center2 );	

				BuildTree((VPF_Node)Node.left,Distances2,Perm2,a2,w2);
			}
			else // build leaf
			{
				Node.Lmin=lmin;
				Node.Lmax=lmax;
				leafs+=a;
				VPF_Leaf L= new VPF_Leaf(a);
				for (int i=0;i<a;i++)
				{
					L.SetValues(this.Remaining[Perm[i]],Distances[i],i);
				}
				//Console.WriteLine("TotalLeafs:{0}",a);
				Node.left=L;
				//Console.WriteLine("Build Leaf:{0}",L.IDs.Length);
			}
		
			Distances2=new double[Perm.Length-1-(a+w-1)];
			Perm2=new int[Perm.Length-1-(a+w-1)];
			for (int i=a+w;i<Perm.Length;i++)
			{	Perm2[i-a-w]=Perm[i];
				//Distances2[i-a-w]=Distances[i];
			}
			if (Divide_Interval(Perm.Length-1-(a+w-1),out a2,out w2,out center2,out Distances2,Perm2)) // build right node		
			{
				Node.Rmin=rmin;
				Node.Rmax=rmax;				
				Node.right=new VPF_Node( center2 );
				BuildTree((VPF_Node)Node.right,Distances2,Perm2,a2,w2);
			}
			else // build leaf
			{
				Node.Rmin=rmin;
				Node.Rmax=rmax;	
				//Console.Write("L:{0} ",this.Remaining[Perm[a+w]]);
				leafs+=Perm.Length-1-(a+w-1);
				VPF_Leaf R=new VPF_Leaf(Perm.Length-1-(a+w-1));
				for(int i=0;i<Perm.Length-1-(a+w-1);i++)
				{
					R.SetValues(this.Remaining[Perm[a+w+i]],Distances[a+w+i],i);
				}
				//Console.WriteLine("Total Leafs:{0}",Perm.Length-1-(a+w-1));
				Node.right=R;
				//Console.WriteLine("Build Leaf:{0}",R.IDs.Length);
			}
		}

		// divide in L, M, and R
		// Returns true  if the division produces a Node
		// Returns false if the division produces a Leaf
		internal bool Divide_Interval(int num_elem,out int a,out int w, out int center, out double[] Distances, int[] Perm)
		{		
			Random r=new Random();

			// Choose random pivot

			center=Remaining[Perm[r.Next(Perm.Length)]];
			Distances=new double[num_elem];
			for (int i=0;i<Perm.Length;i++)
			{
				Distances[i]=this.DB.Dist(this.DB[Remaining[Perm[i]]],this.DB[center]);
			}
			natix.SortingSearching.Sorting.Sort(Distances,Perm);

			if (!BuildingWithTau)
			{
				w=(int)Math.Floor(this.m*num_elem);
				a=(int)Math.Floor((num_elem-w)/2d);			

				if (num_elem<2)
					return false;

				return true;
			}


			w=2;
			a=(int)Math.Floor((num_elem-w)/2d);

			do{


				if (w< num_elem-1 && Distances[ a+w-1 ]-Distances[ a ]>=this.wanted_Tau) 
					return true;
				if (w>=num_elem-1)
				{

					return false;
				}
				w++;
				a=(int)Math.Floor((num_elem-w)/2d);



			}while(BuildingWithTau);

			return false;
		}

		public void Mark2PersistRange(int start,int count,int[] Perm)
		{
			//Console.WriteLine("Persist:{0}",count);
			for (int i=start;i<start+count;i++)
			{
				//Console.WriteLine("P:{0}",i);
				this.Persist[Perm[i]]=true;
			}
		}
		public void RemoveMarkedOnes()
		{
			//Console.WriteLine("Removing. Persist:{0}",this.Persist.Length);
			for (int i=this.Remaining.Count-1;i>=0;i--)
			{
				if (!this.Persist[i])
					this.Remaining.RemoveAt(i);
			}
		}
	
		// Search with arbitrary radius
		public override IResult SearchRange(object q, double radius)
		{ 
			List<ResultPair> results= new List<ResultPair>();
			IResult final_result;
			if (radius <=this.Tau && true)
				final_result= this.SearchTauRange(q,radius);
			else
			{
				foreach (VPF_Tree Tree in this.Forest)
				{
					SearchInNode(q,radius,Tree.Root,results);
				}
				final_result=new Result(results.Count);
				foreach (ResultPair P in results )
					final_result.Push(P.docid,P.dist);
			}

			return final_result;
		}
		internal void SearchInNode(object q,double radius,VPF_Vertex V,List<ResultPair> results)
		{
			if (V is VPF_Node)
				this.SearchInNode(q,radius,(VPF_Node)V,results);
			if (V is VPF_Leaf)
				this.SearchInNode(q,radius,(VPF_Leaf)V,results);
		}
		internal void SearchInNode(object q, double radius, VPF_Leaf V,List<ResultPair> results)
		{
			double d_q_c=DB.Dist(q,DB[V.center]);
			int start=natix.SortingSearching.Search.FindFirst<double>(d_q_c-radius,V.dist2center);
			if (start !=-1)
			{
				int end=natix.SortingSearching.Search.FindLast<double>(d_q_c+radius,V.dist2center);
				if (end !=-1)
				{
					for(int i=start;i<=end;i++)
					{
						double d=this.DB.Dist(q,this.DB[V.IDs[i]]);
						if ( d<=radius ) // left leaf is a result
						{
							results.Add(new ResultPair(V.IDs[i],d) );
						}
					}
				}
			}
		}
		internal void SearchInNode(object q, double radius, VPF_Node V,List<ResultPair> results)
		{
			double d_q_c=this.DB.Dist(q,this.DB[V.center]);

			// check left 

			if ( V.left is VPF_Node ) // left is a node
			{
				VPF_Node L=(VPF_Node) V.left;

				if (/*V.Lmin <= d_q_c+radius &&*/ d_q_c-radius <= V.Lmax ) // Search left
				{
					//Console.Write("L ");
					SearchInNode(q,radius,L,results);
				}
			} else { // left is a leaf
				//Console.WriteLine("V:{0}",V.GetType());
				if (/*V.Lmin <= d_q_c+radius &&*/ d_q_c-radius <= V.Lmax )
				{
					VPF_Leaf L=(VPF_Leaf) V.left;
					//Console.Write("l*");
					for(int i=0;i<L.IDs.Length;i++)
					{
						if ( d_q_c-radius<= L.dist2center[i] && L.dist2center[i]<=d_q_c+radius)
						{
							double d=this.DB.Dist(q,this.DB[L.IDs[i]]);
							if ( d<=radius ) // left leaf is a result
							{
								//Console.WriteLine("F:{0} d:{1}",L.IDs[i],d);
								results.Add(new ResultPair(L.IDs[i],d) );
							}
						}
					}
				}
			}

			// check right

			if ( V.right is VPF_Node ) // right is a node
			{
				VPF_Node R=(VPF_Node) V.right;
				if (V.Rmin <= d_q_c+radius /*&& d_q_c-radius <= V.Rmax*/ ) // Search right
				{
					//Console.Write("R ");
					SearchInNode(q,radius,R,results);
				}

			} else { // right is a leaf
				if (V.Rmin <= d_q_c+radius /*&& d_q_c-radius <= V.Rmax*/ )
				{
					VPF_Leaf R=(VPF_Leaf) V.right;
					//Console.Write("r*");
					for (int i=0;i<R.IDs.Length;i++)
					{
						if ( d_q_c-radius<= R.dist2center[i] && R.dist2center[i]<=d_q_c+radius)
						{
							double d=this.DB.Dist(q,this.DB[R.IDs[i]]);
							if ( d<=radius ) // right leaf is a result
							{
								//Console.WriteLine("F:{0} d:{1}",R.IDs[i],d);
								results.Add(new ResultPair(R.IDs[i],d) );
							}
						}
					}
				}
			}

		}
		public override IResult SearchKNN (object q, int K, IResult res)
		{
			throw new System.NotImplementedException ();
		}
		// Search when radius is less or equal to Tau.
		public IResult SearchTauRange(object q,double radius)
		{
			Console.WriteLine("Searching with Tau.");
			List<ResultPair> results= new List<ResultPair>();

			// Search in every tree
			foreach (VPF_Tree Tree in this.Forest)
			{
				double d_q_c;

				if (Tree.Root is VPF_Node)
				{

					VPF_Node Actual_N=(VPF_Node)Tree.Root;
					//Console.WriteLine();Console.WriteLine("Tree:{0}",this.Forest.IndexOf(Tree));

					do {

						d_q_c=this.DB.Dist(q,this.DB[Actual_N.center]);

						if (Actual_N.left is VPF_Node ) // left is a node
						{

							VPF_Node nextL=(VPF_Node)Actual_N.left;
							//d_q_c=this.DB.Dist(q,this.DB[nextL.center]);	
							if ( /*nextL.min <= d_q_c+radius &&*/ d_q_c-radius <= Actual_N.Lmax ) // go left
							{
								//Console.Write("L");
								Actual_N = nextL;
								continue;
							}

						} else { // left is a leaf
							if (/*nextL.min <= d_q_c+radius &&*/ d_q_c-radius <= Actual_N.Lmax)
							{
								VPF_Leaf LeafL=(VPF_Leaf)Actual_N.left;
								//Console.Write("[{0}:{1}]",LeafL.Elem_id,LeafL.dist2center);
								for (int i=0;i<LeafL.IDs.Length;i++)
								{

									if ( d_q_c-radius<= LeafL.dist2center[i] && LeafL.dist2center[i]<=d_q_c+radius)
									{
										double d=this.DB.Dist(q,this.DB[LeafL.IDs[i]]);
										if ( d<=radius ) // left leaf is a result
										{
											//Console.Write("F");
											results.Add(new ResultPair(LeafL.IDs[i],d) );
										}

									}
								}
							}
						}

						if (Actual_N.right is VPF_Node) // right is a node
						{
							VPF_Node nextR=(VPF_Node)Actual_N.right;
							//d_q_c=this.DB.Dist(q,this.DB[nextR.center]);
							if ( Actual_N.Rmin <= d_q_c+radius /*&& d_q_c-radius <= nextR.max*/) // go right
							{
								//Console.Write("R");
								Actual_N = nextR;
								continue;
							}
						} else { // right is a leaf
							if ( Actual_N.Rmin <= d_q_c+radius /*&& d_q_c-radius <= nextR.max*/)
							{
								VPF_Leaf LeafR=(VPF_Leaf)Actual_N.right;
								//Console.Write("[{0}:{1}]",LeafR.Elem_id,LeafR.dist2center);
								for (int i=0;i<LeafR.IDs.Length;i++)
								{
									if ( d_q_c-radius<=LeafR.dist2center[i] && LeafR.dist2center[i]<=d_q_c+radius )
									{
										double d=this.DB.Dist(q,this.DB[LeafR.IDs[i]]);
										if ( d<=radius ) // right leaf is a result
										{								
											//Console.Write("F");
											results.Add(new ResultPair( LeafR.IDs[i],d));							
										}

									}
								}
							}
						}

						break;
					}while(true);
				}
				else 
				{

					VPF_Leaf Leaf=(VPF_Leaf)Tree.Root;
					SearchInNode(q,radius,Leaf,results	);
					/*for (int i=0;i<Leaf.IDs.Length;i++)
					{
						double d=this.DB.Dist(q,this.DB[Leaf.IDs[i]]);
						if ( d<=radius ) // left leaf is a result
						{
							//Console.Write("F");
							results.Add(new ResultPair(Leaf.IDs[i],d) );
						}
					}*/
				}
			}

			Result final_result=new Result(results.Count);
			foreach (ResultPair P in results )
				final_result.Push(P.docid,P.dist);
			return final_result;
		}

		override public void  Save(BinaryWriter output)
		{
			base.Save (output);

			output.Write (this.Forest.Count);
			foreach (var T in this.Forest) {
				T.Save (output);
			}

			output.Write (this.Tau);
			output.Write (this.leafs);
			Console.WriteLine ("tau:{0},leafs:{1}", this.Tau, this.leafs);
		}

		override public void Load(BinaryReader input)
		{
			base.Load (input);

			int size = input.ReadInt32 ();
			this.Forest = new List<VPF_Tree> (size);
			for (int i = 0; i < size; i++){
				this.Forest.Add (new VPF_Tree());
				this.Forest [i].Load (input);
			}

			this.Tau = input.ReadDouble ();
			this.leafs = input.ReadInt32 ();
			Console.WriteLine ("tau:{0},leafs:{1}", this.Tau, this.leafs);
		}


	}
}

