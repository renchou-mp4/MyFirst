---
description: "Use when writing, reviewing, or editing Unity C# scripts. Covers naming conventions, code structure, performance rules, tech stack restrictions, and comment standards for Unity 2022.3 LTS projects."
applyTo: "**/*.cs"
---

# Unity C# 编码规范

## 命名规范

### 变量命名

| 类型 | 规则 | 示例 |
|------|------|------|
| 私有成员字段 | `_` 前缀 + 小驼峰 | `_itemCount`, `_scrollView` |
| 公有属性 | 大驼峰（PascalCase） | `ItemCount`, `ScrollView` |
| Unity 序列化字段 | `[SerializeField]` + 同私有字段规则 | `[SerializeField] float _itemHeight;` |
| Unity 特有类型 | 类型缩写前缀 + `_` + 大驼峰 | `Tf_Content`, `Go_ItemPrefab`, `Img_Icon`, `Btn_Confirm` |
| 局部变量 | 小驼峰 | `itemData`, `scrollPosition` |
| 常量 | 全大写 + 下划线 | `MAX_ITEM_COUNT`, `DEFAULT_SIZE` |
| 接口 | `I` 前缀 | `IDataProvider`, `IScrollHandler` |
| 枚举 | `E` 前缀 | `EItemType`, `EScrollDirection` |
| 事件 | `Event` 后缀 | `OnCompleteEvent`, `OnItemClickEvent` |
| 委托 | `Delegate` 后缀 | `ItemClickDelegate`, `OnDataLoadedDelegate` |

### Unity 类型缩写表（常用）

| 类型 | 缩写 |
|------|------|
| `Transform` | `Tf` |
| `RectTransform` | `Rtf` |
| `ScrollRect` | `Sr` |
| `GameObject` | `Go` |
| `Image` | `Img` |
| `Button` | `Btn` |
| `Text` / `TMP_Text` | `Txt` |
| `Slider` | `Sld` |
| `Toggle` | `Tgl` |
| `Canvas` | `Cvs` |
| `AudioSource` | `Aud` |
| `Animator` | `Anim` |
| `ParticleSystem` | `Ps` |
| `Collider` | `Col` |
| `Rigidbody` | `Rb` |

---

## 变量定义顺序

在同一功能区域内，按以下顺序排列：

```csharp
// ── 常量 (CONSTANTS) ───────────────────────────────────────
private const int MAX_ITEM_COUNT = 100;  // 最大条目数量

// ── 序列化字段 (Serialized Fields) ────────────────────────
[SerializeField]
private float _itemHeight = 100f;        // Item 高度

// ── 公共属性 (Properties) ──────────────────────────────────
public float ItemHeight => _itemHeight;

// ── 私有字段 (Private Fields) ──────────────────────────────
private int _currentIndex;              // 当前选中索引

// ── 事件 (Events) ─────────────────────────────────────────
public event Action<int> OnItemClickEvent;
```

---

## 代码结构规范

- **单文件行数不超过 1000 行**，超过则拆分为分部类（`partial class`），新文件命名为 `主类名.子功能.cs`
- **单方法行数不超过 200 行**，超过则提取为私有方法
- **if 嵌套不超过 3 层**，超出则提取为独立方法或使用 Guard Clause 提前返回
- **禁止在同一脚本内嵌套定义类**，拆分为分部类或独立文件

### MonoBehaviour 生命周期顺序

```
Awake → OnEnable → Start → Update → FixedUpdate → LateUpdate → OnDisable → OnDestroy
```

---

## 性能规范

- **禁止**在 `Update` / `FixedUpdate` / `LateUpdate` 中创建对象（`new`、`Instantiate`）
- 频繁创建销毁的对象**必须使用对象池**
- `GetComponent`、`Find`、`FindObjectOfType` 的结果**必须缓存**，禁止每帧调用
- 字符串拼接使用 `StringBuilder`，禁止在热路径使用 `+` 操作符
- 性能敏感代码必须添加注释说明原因

---

## 技术栈限制

| 禁止 | 替代方案 |
|------|---------|
| 运行时 LINQ | 手写循环（Editor 脚本中允许 LINQ） |
| `StartCoroutine` / `IEnumerator` | **UniTask** |
| `Animator` 控制动画过渡 | **DOTween** |
| `ScreenPointToLocalPointInRectangle` 等昂贵 UI 计算 | 使用 `RectTransform` 直接操作 |

---

## 注释规范

- **所有公共方法**必须有 XML 文档注释（中文内容）
- **所有字段、常量**必须有行内中文注释
- **复杂逻辑**必须有中文说明
- **集合类型**需注明存储内容含义
- **Attribute 单独占一行**，不与声明写在同一行

```csharp
/// <summary>
/// 初始化列表视图，设置数据总量和 Item 创建回调。
/// </summary>
/// <param name="totalCount">数据总条数，-1 表示无限列表</param>
/// <param name="onGetItem">按索引获取 Item 的回调</param>
public void Init(int totalCount, Func<int, ItemView> onGetItem)
{
    // 无限列表模式下不支持 ScrollBar
    _supportScrollBar = totalCount >= 0;
    ...
}
```

---

## 日志规范

- 所有日志（`Debug.Log` / `Debug.LogWarning` / `Debug.LogError`）内容**必须**采用统一格式：`类名---方法名---具体log`
- 分隔符为三个连字符 `---`，前后**不带空格**
- `具体log` 部分保留原有内容与插值，仅在最前面追加 `类名---方法名---` 前缀

```csharp
// 正确
Debug.LogError($"LoopListView---InitItemPool---【{data.Prefab.name}】对象池重复创建！");

// 错误（缺少类名/方法名前缀）
Debug.LogError($"【{data.Prefab.name}】对象池重复创建！");
```

---

## 版本兼容性

- 所有 API 必须在 **Unity 2022.3 LTS** 及以上版本可用
- 目标平台：StandaloneOSX、StandaloneWindows64、**Android、iOS**（需考虑移动端性能）
- 涉及第三方库时，确认其与 Unity 2022.3 + IL2CPP 的兼容性
