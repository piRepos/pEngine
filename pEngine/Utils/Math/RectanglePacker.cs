using System;
using System.Linq;
using System.Collections.Generic;

namespace pEngine.Utils.Math
{
	/// <summary>
	/// This class manage the storage of either <see cref="ISized"/> objects
	/// by packing them changing the position in a storage box that has a size too.
	/// </summary>
	/// <typeparam name="T">Type of object to store.</typeparam>
	public class RectanglePacker<T> where T : IMovableObject, ISized
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:pEngine.Base.DataStructs.GenericAtlas`1"/> class.
		/// </summary>
		public RectanglePacker()
		{
			NotFitted = new List<T>();
			Objects = new List<T>();
			pTree = new Block();
		}

		/// <summary>
		/// Gets the used space.
		/// </summary>
		/// <value>The used space.</value>
		public Vector2i UsedSpace => pTree.Size;

		/// <summary>
		/// Occurs when element position changes.
		/// </summary>
		public event EventHandler<T> OnPositionChange;

		/// <summary>
		/// Occurs when an element can't be fitted.
		/// </summary>
		public event EventHandler<T> OnElementNotFit;

		#region Position management

		/// <summary>
		/// Gets the objects added.
		/// </summary>
		public List<T> Objects { get; internal set; }

		/// <summary>
		/// Gets the objects not fitted.
		/// </summary>
		public List<T> NotFitted { get; internal set; }

		/// <summary>
		/// Gets or sets the spacing between each object.
		/// </summary>
		public Vector2i Spacing { get; set; } = Vector2i.Zero;

		/// <summary>
		/// Adds the object to the fit list.
		/// </summary>
		/// <param name="Obj">Object.</param>
		public virtual void AddObjects(params T[] objs)
		{
			foreach (var obj in objs)
			{
				if (!Objects.Contains(obj))
					Objects.Add(obj);
			}
		}

		/// <summary>
		/// Remove the specified bound.
		/// </summary>
		/// <param name="Obj">Object.</param>
		public virtual void Remove(T Obj)
		{
			if (Objects.Contains(Obj))
				Objects.Remove(Obj);
		}

		#endregion

		#region Algorithm

		/// <summary>
		/// Tree struct for rectangle split abstraction.
		/// </summary>
		private class Block
		{
			public Vector2i Position;
			public Vector2i Size;
			public Block Right, Bottom;
			public bool Used;

			public void MakeLeafs()
			{
				Right = new Block();
				Bottom = new Block();
				Right.Used = false;
				Bottom.Used = false;
			}
		}

		/// <summary>
		/// Fit all elements in the area.
		/// </summary>
		public virtual void Fit()
		{
			// If there are no object nothink to do.
			if (Objects.Count == 0)
				return;
			
			// Sort all elements for height.
			Objects = Objects.OrderByDescending(X => X.Size.Height).ToList();

			// Add first object.
			pTree.Size = Objects[0].Size + Spacing;

			Block Node;

			// Foreach object try to fit it.
			foreach (T B in Objects)
			{
				// If we found the space for fit this object
				if ((Node = FindNode(pTree, B.Size + Spacing)) != null)
				{
					// Split the current node and set the new position
					Node = SplitNode(Node, B.Size + Spacing);

					if (B.Position != Node.Position)
						OnPositionChange?.Invoke(this, B);
					
					B.Position = Node.Position;
				}
				else
				{
					// Else grow the atlas
					Node = GrowNode(B.Size + Spacing);

					// If we can't grow the atlas, this item can't be added
					if (Node == null)
					{
						if (!NotFitted.Contains(B))
							NotFitted.Add(B);
						
						OnElementNotFit?.Invoke(this, B);
						
						continue;
					}

					if (B.Position != Node.Position)
						OnPositionChange?.Invoke(this, B);

					// Else space found!
					B.Position = Node.Position;
				}

			}
		}

		#endregion

		#region Node management

		/// <summary>
		/// Entry point.
		/// </summary>
		private Block pTree;

		/// <summary>
		/// Finds a free node.
		/// </summary>
		/// <returns>Reference to a free node.</returns>
		/// <param name="Root">Tree root.</param>
		/// <param name="Dim">Size requested.</param>
		Block FindNode(Block Root, Vector2i Dim)
		{
			Block Tmp;
			if (Root.Used)
			{
				if ((Tmp = FindNode(Root.Right, Dim)) != null) return Tmp;
				else return FindNode(Root.Bottom, Dim);
			}
			else if (Dim.Width <= Root.Size.Width && Dim.Height <= Root.Size.Height)
				return Root;
			else return null;
		}

		/// <summary>
		/// Splits a node into 2 parts.
		/// </summary>
		/// <returns>Source node splitted.</returns>
		/// <param name="Node">Node to split.</param>
		/// <param name="Dim">Split size.</param>
		Block SplitNode(Block Node, Vector2i Dim)
		{
			Node.Used = true;

			if (Node.Right == null || Node.Bottom == null)
				Node.MakeLeafs();

			Node.Bottom.Position = new Vector2i(Node.Position.X, Node.Position.Y + Dim.Height);
			Node.Bottom.Size = new Vector2i(Node.Size.Width, Node.Size.Height - Dim.Height);

			Node.Right.Position = new Vector2i(Node.Position.X + Dim.Width, Node.Position.Y);
			Node.Right.Size = new Vector2i(Node.Size.Width - Dim.Width, Dim.Height);

			return Node;
		}

		/// <summary>
		/// Grows the node size.
		/// </summary>
		/// <returns>The node to grow.</returns>
		/// <param name="Dim">New size.</param>
		Block GrowNode(Vector2i Dim)
		{
			bool CanGrowBottom = Dim.Width <= pTree.Size.Width;
			bool CanGrowRight = Dim.Height <= pTree.Size.Height;

			bool ShouldGrowRight = CanGrowRight && (pTree.Size.Height >= pTree.Size.Width + Dim.Width);
			bool ShouldGrowBottom = CanGrowBottom && (pTree.Size.Width >= pTree.Size.Height + Dim.Height);

			if (ShouldGrowRight) return GrowRight(Dim);
			if (ShouldGrowBottom) return GrowDown(Dim);
			if (CanGrowRight) return GrowRight(Dim);
			if (CanGrowBottom) return GrowDown(Dim);

			return null;
		}

		/// <summary>
		/// Grows the right part of a node.
		/// </summary>
		/// <returns>Node grown.</returns>
		/// <param name="Dim">New size.</param>
		Block GrowRight(Vector2i Dim)
		{
			Block Node = new Block();
			Node.Bottom = pTree;
			Node.Right = new Block();
			pTree = Node;

			if (pTree.Right == null || pTree.Bottom == null)
				pTree.MakeLeafs();

			pTree.Size = new Vector2i(Node.Bottom.Size.Width + Dim.Width, Node.Bottom.Size.Height);

			pTree.Right.Position = new Vector2i(pTree.Size.Width - Dim.Width, 0);
			pTree.Right.Size = new Vector2i(Dim.Width, pTree.Size.Height);

			pTree.Used = true;

			if ((Node = FindNode(pTree, Dim)) != null)
				return SplitNode(Node, Dim);
			else
				return null;
		}

		/// <summary>
		/// Grows the bottom part of a node.
		/// </summary>
		/// <returns>Node grown.</returns>
		/// <param name="Dim">New size.</param>
		Block GrowDown(Vector2i Dim)
		{
			Block Node = new Block();
			Node.Right = pTree;
			Node.Bottom = new Block();
			pTree = Node;

			if (pTree.Right == null || pTree.Bottom == null)
				pTree.MakeLeafs();

			pTree.Size = new Vector2i(Node.Right.Size.Width, Node.Right.Size.Height + Dim.Height);

			pTree.Bottom.Position = new Vector2i(0, pTree.Size.Height - Dim.Height);
			pTree.Bottom.Size = new Vector2i(pTree.Size.Width, Dim.Height);

			pTree.Used = true;

			if ((Node = FindNode(pTree, Dim)) != null)
				return SplitNode(Node, Dim);
			else
				return null;

		}

        #endregion
	}
}
