//
//  Copyright 2013     Eric Sadit Tellez Avila
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
//

using System;
using System.IO;
using natix.CompactDS;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using natix;
using natix.SortingSearching;
using System.Threading;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public abstract class LocalSearch : BasicIndex
	{
//		protected class SearchState
//		{
//			public HashSet<int> evaluated = new HashSet<int> ();
//			public IResult res;
//			public SearchState(IResult res) {
//				this.res = res;
//			}
//		}

		public class Vertex : List<int>
		{
			public Vertex(Vertex v) : base(v) {
			}
			public Vertex(int capacity) : base(capacity) {
			}
		}

		public DictionaryRandomAccess<int,Vertex> Vertices;
		public int Neighbors;

		protected Random rand = new Random ();
		
		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.Neighbors = Input.ReadInt32 ();
			var count = Input.ReadInt32 ();
			this.Vertices = new DictionaryRandomAccess<int, Vertex> (count);
			for (int i = 0; i < count; ++i) {
				var objID = Input.ReadInt32 ();
				var c = Input.ReadInt32 ();
				var vertex = new Vertex (c);
				PrimitiveIO<int>.LoadVector(Input, c, vertex);
				this.Vertices.Add (objID, vertex);
			}
		}
		
		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write (this.Neighbors);
			Output.Write ((int) this.Vertices.Count);
			foreach (var p in this.Vertices) {
				Output.Write ((int) p.Key);
				var vertex = p.Value;
				Output.Write ((int) vertex.Count);
				PrimitiveIO<int>.SaveVector (Output, vertex);
			}
		}

		public void CloneVertices()
		{
			this.Vertices = new DictionaryRandomAccess<int, Vertex> (this.Vertices);
		}

		public void DropLargeLinks()
		{
			foreach (var p in this.Vertices) {
				var objID = p.Key;
				var vertex = p.Value;
				var obj = this.DB [objID];
				var queue = new Result (this.Neighbors);
				foreach (var linkToID in this.Vertices[objID]) {
					var dist = this.DB.Dist (obj, this.DB [linkToID]);
					queue.Push (linkToID, dist); 
				}
				vertex.Clear ();
				foreach (var u in queue) {
					vertex.Add (u.ObjID);
				}
				vertex.TrimExcess();
			}
		}

		protected void InternalBuild (MetricDB db, int neighbors)
		{
			this.DB = db;
			this.Neighbors = neighbors;
			int n = db.Count;
			this.Vertices = new DictionaryRandomAccess<int, Vertex> (n);
			for (int objID = 0; objID < n; ++objID) {
				if (objID % 10000 == 0) {
					Console.WriteLine ("XXX==== {0} DB: {1}, Neighbors: {2}, objID: {3}/{4}, timestamp: {5}", 
						this, Path.GetFileName(db.Name), neighbors, objID, db.Count, DateTime.Now);
				}
				this.AddObjID(objID);
			}
		}
		
		protected void AddObjID(int objID)
		{
			if (this.Vertices.Count <= this.Neighbors) {
				// the first items are just connected among them
				int c = this.Vertices.Count;
				var v = new Vertex (this.Neighbors);
				this.Vertices.Add (objID, v);
				for (int i = 0; i < c; ++i) {
					this.Vertices [i].Add (c);
					this.Vertices [c].Add (i);
				}
			} else {
				var res = new Result (this.Neighbors);
//				if (this.RepeatSearch > 1 && this.Vertices.Count > 1000000) {
//					this.ParallelSearchKNN (this.DB[objID], this.Neighbors, res);
//				} else {
					this.SearchKNN (this.DB[objID], this.Neighbors, res);
//				}
				var v = new Vertex(this.Neighbors*2);
				this.Vertices.Add(objID, v);
				foreach (var p in res) {
					v.Add(p.ObjID);
					this.Vertices[p.ObjID].Add(objID);
				}
			}
		}


		//Erase an object in the Graph. (taking far elements)
		public void Erase(int obj_id)
		{
			/* 
			// modificar 


			//if (obj_id<=this.Neighbors)
			//	return;

			int num_neighbors = this.Vertices[obj_id].Count;
			//Console.WriteLine("Num-neighbors:{0}",num_neighbors);

			// Move all the edges that point to the object and that were not created when inserted obj_id.
			if (num_neighbors > this.Neighbors) 
			{
				for (int i=this.Neighbors;i<num_neighbors;i++)
				{
					int point_id=this.Vertices[obj_id][i];
					int edge2move=0;
					// find the edge to move
					edge2move=this.Vertices[point_id].FindIndex(x => x==obj_id);
					if (this.Vertices[point_id][edge2move]!=obj_id)
						Console.WriteLine("Wrong edge");

					// Find the new end of the edge (Find the farthest neighbor of obj_id from point_id).
					int farthest=-1;
					double max_dist=0;
					for (int j=0;j<num_neighbors;j++)
					{
						double dist=this.DB.Dist(DB[point_id],DB[this.Vertices[obj_id][j]]);
						if (max_dist < dist && this.Vertices[obj_id][j]!=obj_id && this.Vertices[obj_id][j]!=point_id &&
							!this.Vertices[point_id].Contains(this.Vertices[obj_id][j]) )
						{
							max_dist=dist;
							farthest=this.Vertices[obj_id][j];

						}
					}
					if ( farthest==-1 ) // remove the edge
						this.Vertices[point_id].RemoveAt(edge2move);
					else   // Move the edge 
					{
						this.Vertices[point_id][edge2move]=farthest;
						// Add the edge in opposite direction
						this.Vertices[farthest].Add(point_id);
					}

				}
			}
			// Remove all the edges that point to the object 
			for (int i=0;i<Math.Min( this.Neighbors,num_neighbors);i++)
			{
				this.Vertices[this.Vertices[obj_id][i]].Remove(obj_id);
			}
			// Remove all the edges from obj_id
			this.Vertices[obj_id]=null;
			*/
		}

		//Erase an object in the Graph.
		// The difference is that it takes the closest element (from the neighbors of obj_id) when we redirect the edges.
		public void Erase_take_close(int obj_id)
		{
			/*
			modificar de a cuerdo a los cambios


			//if (obj_id<=this.Neighbors)
			//	return;


			int num_neighbors = this.Vertices[obj_id].Count;
			//Console.WriteLine("Num-neighbors:{0}",num_neighbors);

			// Move all the edges that point to the object and that were not created when inserted obj_id.
			if (num_neighbors > this.Neighbors) 
			{
				for (int i=this.Neighbors;i<num_neighbors;i++)
				{
					int point_id=this.Vertices[obj_id][i];
					int edge2move=0;
					// find the edge to move
					edge2move=this.Vertices[point_id].FindIndex(x => x==obj_id);

					// Find the new end of the edge (Find the closest neighbor of obj_id from point_id).
					int closest=-1;
					double min_dist=double.MaxValue;
					for (int j=0;j<num_neighbors;j++)
					{
						double dist=this.DB.Dist(DB[point_id],DB[this.Vertices[obj_id][j]]);
						if (min_dist > dist && this.Vertices[obj_id][j]!=obj_id && this.Vertices[obj_id][j]!=point_id
							&& !this.Vertices[point_id].Contains( this.Vertices[obj_id][j] ) )
						{
							min_dist=dist;
							closest=this.Vertices[obj_id][j];
						}
					}

					if ( closest==-1 ) // remove the edge
						this.Vertices[point_id].RemoveAt(edge2move);
					else   // Move the edge 
					{
						this.Vertices[point_id][edge2move]=closest;
						// Add the edge in opposite direction
						this.Vertices[closest].Add(point_id);
					}
				}
			}
			// Remove all the edges that point to the object
			for (int i=0;i<Math.Min( this.Neighbors,num_neighbors);i++)
			{
				this.Vertices[this.Vertices[obj_id][i]].Remove(obj_id);
			}
			// Remove all the edges from obj_id
			this.Vertices[obj_id]=null;
			*/
		}


		//Erase an object in the Graph.
		// The difference is that it takes the closest element (amoung all points) when we redirect the edges.
		public void Erase_take_closest(int obj_id)
		{
			/* 
			// modificar de acuerdo a los cambios


			//if (obj_id<=this.Neighbors)
			//	return;

			int num_neighbors = this.Vertices[obj_id].Count;
			//Console.WriteLine("Num-neighbors:{0}:id:{1}",num_neighbors,obj_id);


			// Move all the edges that point to the object and that were not created when inserted obj_id.
			if (num_neighbors > this.Neighbors) 
			{
				for (int i=this.Neighbors;i<num_neighbors;i++)
				{
					if ( i<this.Neighbors && this.Vertices[this.Vertices[obj_id][i]].Count>this.Neighbors)
						continue;

					int point_id=this.Vertices[obj_id][i];
					int edge2move=0;
					// find the edge to move
					edge2move=this.Vertices[point_id].FindIndex(x => x==obj_id);

					// Find the new end of the edge (Find the closest neighbor of obj_id from all points).
					int closest=0;
					//double min_dist=double.MaxValue;

					IResult res=new Result(2);

					// fill the state with neighbors of point_id
					SearchState exclude=new SearchState();
					foreach (int e in this.Vertices[point_id])
						exclude.visited.Add(e);


					//Console.WriteLine("obj_id:{0}",obj_id);
					for (int j=0;j<num_neighbors;j++)
					{
						if (this.Vertices[obj_id][j]!=point_id )
						{
							//Console.WriteLine("Greedy(q:{0} start:{1})",point_id,this.Vertices[obj_id][j]);
							this.GreedySearch(this.DB[point_id],res,this.Vertices[obj_id][j],null,exclude);
							//Console.WriteLine("Res:{0}",res.Count);
							//if (res.Count!=0)
							//	Console.WriteLine("NN:{0} dist:{1} good:{2}",res.First.docid, res.First.dist,!exclude.visited.Contains(res.First.docid));

						}
					}
					// que pasa cuando todos los lugares para empezar la búsqueda están prohibidos?
					if (res.Count==0)
					{

						closest=NN(point_id);
						// Move the edge 
						int t=this.Vertices[point_id].FindIndex(x=>x==closest);

						this.Vertices[point_id][edge2move]=closest;
						this.Vertices[point_id].RemoveAt(t);

						//if (this.Vertices[point_id].Count < this.Neighbors)
						{
							//	Console.WriteLine("Better not delete");
							//	Console.ReadLine();
						}
					}
					else
					{
						closest=res.First.docid;

						if (closest==point_id)
						{
							Console.WriteLine("ERRRRROOOOOOOOOORRRRRRRRR");
							Console.ReadLine();
						}


						// Move the edge 
						this.Vertices[point_id][edge2move]=closest;
						// Add the edge in opposite direction
						this.Vertices[closest].Add(point_id);
					}
				}
			}
			// Remove all the edges that point to the object.
			for (int i=0;i<Math.Min( this.Neighbors,num_neighbors);i++)
			{
				this.Vertices[this.Vertices[obj_id][i]].Remove(obj_id);
			}
			// Remove all the edges from obj_id
			this.Vertices[obj_id]=null;
			*/
		}

		// Erase a lot of points.
		public void EraseRange(int start,int number)
		{
			for (int i=start;i<start+number;i++)
			{
				this.Erase(i);
			}
		}

	}
}