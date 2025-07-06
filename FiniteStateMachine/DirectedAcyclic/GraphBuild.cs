 
namespace FiniteStateMachine.DirectedAcyclic;
/// <summary>
/// 图构建器
/// </summary>
/// <typeparam name="TValue">值类型</typeparam> 
public class GraphBuild<TValue> where TValue : notnull
{
	private Dictionary<TValue, GraphNodeBuild<TValue>> Nodes { get; } = new Dictionary<TValue, GraphNodeBuild<TValue>>();

	/// <summary>
	/// 获取所有节点
	/// </summary>
	public IReadOnlyCollection<GraphNodeBuild<TValue>> ReadOnlyNodes => Nodes.Values;
	/// <summary>
	/// 添加节点
	/// </summary>
	/// <param name="value">节点储存值</param>
	public void AddNode(TValue value)
	{
		var node = new GraphNodeBuild<TValue> { Value = value };
		node.ChangedValue += ReplaceNodeValue;
		Nodes.Add(value, node);
	}
	/// <summary>
	/// 移除节点
	/// </summary>
	/// <param name="value"></param>
	public void RemoveNode(TValue value)
	{
		var node = Nodes[value];
		node.ChangedValue -= ReplaceNodeValue;
		Nodes.Remove(value);
	}


	private void ReplaceNodeValue(TValue oldValue, TValue newValue)
	{
		var node = Nodes[oldValue];
		Nodes.Remove(oldValue);
		Nodes.Add(newValue, node);
	}

	/// <summary>
	/// 添加边
	/// </summary>
	/// <param name="from">源点</param>
	/// <param name="to">目标点</param>
	/// <exception cref="ArgumentException">没有收录这个值</exception>
	public void AddDirectedEdge(TValue from, TValue to)
	{
		if (!Nodes.TryGetValue(from, out var fromNode) || !Nodes.TryGetValue(to, out var toNode))
		{
			throw new ArgumentException("One or both nodes not found");
		}
		fromNode.OutEdges.Add(toNode);
	}

	/// <summary>
	/// 移除边
	/// </summary>
	/// <param name="from">源点</param>
	/// <param name="to">目标点</param> 
	/// <exception cref="ArgumentException">没有收录这个值</exception>
	public void RemoveDirectedEdge(TValue from, TValue to)
	{
		if (!Nodes.TryGetValue(from, out var fromNode) || !Nodes.TryGetValue(to, out var toNode))
		{
			throw new ArgumentException("One or both nodes not found");
		}
		fromNode.OutEdges.Remove(toNode);
	}

	/// <summary>
	/// 获取节点
	/// </summary>
	/// <param name="value">节点值</param>
	/// <returns>节点（若收录）</returns>
	public GraphNodeBuild<TValue>? GetNode(TValue value)
	{
		return Nodes.GetValueOrDefault(value);
	}

	/// <summary>
	/// 判断是否存在节点
	/// </summary>
	/// <param name="value">节点值</param>
	/// <returns></returns>
	public bool ContainsNode(TValue value)
	{
		return Nodes.ContainsKey(value);
	}


	/// <summary>
	/// 判断是否存在边
	/// </summary>
	/// <param name="from">源点</param>
	/// <param name="to">目标点</param> 
	public bool ContainsEdge(TValue from, TValue to)
	{
		var fromNode = GetNode(from);
		var toNode = GetNode(to);
		return fromNode != null && toNode != null && fromNode.OutEdges.Contains(toNode);
	}

	/// <summary>
	/// 构建不可变图
	/// </summary>
	/// <returns></returns>
	public Graph<TValue> Build()
	{
		// 节点映射表：用于快速查找原始节点对应的可变节点
		var nodeMapping = new Dictionary<TValue, GraphNodeMutable<TValue>>();

		// 第一遍遍历：创建所有可变节点实例
		// --------------------------------------------
		foreach (var node in Nodes)
		{
			var mutableNode = new GraphNodeMutable<TValue>
			{
				BuildValue = node.Value.Value  // 保留原始节点值
			};
			nodeMapping.Add(node.Value.Value, mutableNode);
		}

		// 第二遍遍历：建立边关系
		// --------------------------------------------
		foreach (var node in Nodes)
		{
			var sourceNode = nodeMapping[node.Value.Value];  // 获取源节点

			foreach (var edge in node.Value.OutEdges)
			{
				// 仅当目标节点存在时才建立连接（防御性编程）
				if (nodeMapping.TryGetValue(edge.Value, out var targetNode))
				{
					// 无权图：直接添加目标节点到 HashSet（无需权重）
					sourceNode.OutEdges.Add(targetNode);  // 类型改为 HashSet<GraphNode<TValue>>

					// 同时维护目标节点的入边集合（双向关系）
					targetNode.InEdges.Add(sourceNode);   // InEdges 仍可用 List/HashSet
				}
			}
		}

		// 创建可变图并组织节点
		// --------------------------------------------
		var mutableGraph = new GraphMutable<TValue>();
		foreach (var mutableNode in nodeMapping.Values)
		{
			mutableGraph.Nodes.Add(mutableNode);

			// 节点分类逻辑
			if (mutableNode.OutEdges.Count == 0 && mutableNode.InEdges.Count == 0)
			{
				mutableGraph.IsolatedNodes.Add(mutableNode);  // 孤立节点（无任何连接）
			}
			else if (mutableNode.OutEdges.Count == 0)
			{
				mutableGraph.SinkNodes.Add(mutableNode);  // 汇点（只有入边）
			}
			else if (mutableNode.InEdges.Count == 0)
			{
				mutableGraph.SourceNodes.Add(mutableNode);  // 源点（只有出边）
			}
		}

		// 计算强连通分量
		// --------------------------------------------
		HashSet<HashSet<GraphNode<TValue>>> outReachable = new();
		HashSet<GraphNode<TValue>> inReachable = new();

		// 从所有源节点开始DFS遍历（保证覆盖所有连通区域）
		foreach (var source in mutableGraph.SourceNodes.Cast<GraphNodeMutable<TValue>>())
		{
			ComponentMutable<TValue>? component = null;
			RecalculateOutReachable(source, outReachable, inReachable, mutableGraph, 0, ref component);
			if (component != null)
			{
				mutableGraph.Components.Add(component);
			}
		}

		// 处理孤立节点（每个孤立节点自成独立分量）
		foreach (var isolated in mutableGraph.IsolatedNodes.Cast<GraphNodeMutable<TValue>>())
		{
			ComponentMutable<TValue>? component = null;
			RecalculateOutReachable(isolated, outReachable, inReachable, mutableGraph, 0, ref component);
			mutableGraph.Components.Add(component!);
		}

		return mutableGraph;
	}

	/// <summary>
	/// 递归计算节点的可达性并构建强连通分量
	/// </summary>
	/// <param name="node">当前处理的节点</param>
	/// <param name="outReachable">维护当前路径上的所有出边可达集合</param>
	/// <param name="inReachable">维护当前路径上的所有入边可达集合</param>
	/// <param name="level">当前递归深度（用于层级控制）</param>
	/// <param name="component">当前正在构建的强连通分量</param>
	private static void RecalculateOutReachable(
		GraphNodeMutable<TValue> node,
		HashSet<HashSet<GraphNode<TValue>>> outReachable,
		HashSet<GraphNode<TValue>> inReachable,
		GraphMutable<TValue> mutableGraph,
		int level,
		ref ComponentMutable<TValue>? component)
	{
		// 环检测：如果当前节点已在当前路径中，说明存在环
		if (inReachable.Contains(node))
		{
			throw new ArgumentException($"Cycle detected at node {node.BuildValue}");
		}

		// 已处理节点优化
		if (node.BuildComponent != null)
		{
			component = node.BuildComponent;  // 继承已有分量

			// 将当前节点的可达性传播到上游
			foreach (var upstreamSet in outReachable)
			{
				upstreamSet.UnionWith(node.OutReachableNodes);
			}

			// 将当前路径的入边可达性传播到下游
			foreach (var downstreamNode in node.OutReachableNodes.Cast<GraphNodeMutable<TValue>>())
			{
				downstreamNode.InReachableNodes.UnionWith(inReachable);
			}

			// 如果不需要更新层级则提前返回
			if (level <= node.BuildLevel)
			{
				return;
			}
		}

		// 更新节点层级信息
		if (node.BuildLevel < level)
		{
			node.BuildLevel = level;
		}

		// 将当前节点加入当前路径的可达集合
		// --------------------------------------------------
		outReachable.Add(node.OutReachableNodes);
		inReachable.Add(node);

		// 将当前节点添加到所有上游的可达集合中
		foreach (var upstreamSet in outReachable)
		{
			upstreamSet.Add(node);
		}

		// 将当前路径的入边可达性记录到当前节点
		node.InReachableNodes.UnionWith(inReachable);

		// 递归处理所有邻居节点
		// --------------------------------------------------
		foreach (var neighbor in node.OutEdges.Cast<GraphNodeMutable<TValue>>())
		{
			RecalculateOutReachable(neighbor, outReachable, inReachable, mutableGraph, level + 1, ref component);
		}

		// 回溯清理：将当前节点移出路径集合
		// --------------------------------------------------
		outReachable.Remove(node.OutReachableNodes);
		inReachable.Remove(node);

		// 确保组件存在并将当前节点加入组件
		component ??= new ComponentMutable<TValue>() { GraphBuild = mutableGraph };
		component.Nodes.Add(node);
		node.BuildComponent = component;  // 标记节点所属分量
	}

}