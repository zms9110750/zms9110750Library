# TreeCollection 使用说明

## 基本树结构操作

### 创建树和添加节点
```csharp
// 创建根节点
var root = new TreeNode<string>("根节点");

// 批量添加子节点
root.Add(["子节点1", "子节点2", "子节点3"]);

// 嵌套添加子节点
root[0].Add("子节点1.1", "子节点1.2");  // 通过索引访问
root["子节点2"].Add("子节点2.1");       // 通过值访问
```

### 节点操作
```csharp
// 移动节点到新父节点
root.Add(root[0][1]);  // 将"子节点1.2"移动到根下

// 删除节点
root.RemoveAt(1);      // 删除第二个子节点

// 获取根节点和深度
var rootNode = root.Root;  // 获取根节点
int depth = root.Depth;    // 获取当前节点深度
```

### 切片操作
```csharp
// 获取前两个子节点
var slice = root[..2];  

// 遍历切片
foreach(var node in slice)
{
    Console.WriteLine(node.Value);
}
```

## 字典树(Trie)操作

```csharp
var trie = new Trie();

// 添加单词
trie.Add("apple");
trie.Add("application");
trie.Add("banana");

// 搜索前缀
foreach(var word in trie.Search("app"))
{
    Console.WriteLine(word); // 输出 apple, application
}

// 复杂搜索示例
/*
a b匹配:
ac b
ac cb b
不匹配:
ab
ac cb
*/
```

## 高级树操作

### 可视化树结构
```csharp
// 输出树形结构
Console.WriteLine(root.ToString());
/*
输出示例:
根节点
├─ 子节点1
│  ├─ 子节点1.1
│  └─ 子节点1.2
├─ 子节点2
│  └─ 子节点2.1
└─ 子节点3
*/
```

### 批量操作
```csharp

// 批量删除节点
root.RemoveAll(node => node.Value.StartsWith("子节点"));
```

这个库提供了极其简单直观的API来处理各种树结构，支持批量操作、切片访问和直观的树形可视化输出。