---
name: unity-cs-format-review
description: "Use when: reviewing Unity C# code format, checking naming conventions, auditing code style, validating Unity script structure. Checks against project coding standards: naming prefixes, variable order, comment rules, attribute placement, tech stack restrictions. Does NOT check logic correctness."
argument-hint: "可选：粘贴要审查的代码，或直接说「审查当前文件」"
user-invocable: true
disable-model-invocation: true
---

# Unity C# 格式审查

## 职责边界

- **只修复格式与规范**，不评价代码逻辑是否正确或合理
- **直接修改文件**，不在聊天框输出修改内容，不需要用户确认
- 审查依据：`.github/instructions/unity-csharp.instructions.md`

---

## 审查流程

### Step 1 — 确认审查目标

- 若用户粘贴了代码片段：直接审查
- 若用户说「审查当前文件」：读取当前编辑器打开的文件
- 若用户指定了文件路径：读取该文件

### Step 2 — 逐项检查（按以下顺序）

#### ① 命名规范

| 类型 | 规则 | 违规示例 |
|------|------|---------|
| 私有成员字段 | `_` 前缀 + 小驼峰 | `mItemCount`、`itemCount` |
| 公有属性 | PascalCase，无 `_` 前缀 | `item_Count`、`itemCount` |
| 序列化字段 | `[SerializeField]` + 私有字段规则 | `public float itemHeight;` |
| Unity 类型字段 | 类型缩写前缀 + `_` + PascalCase | `Transform content`、`tfContent` |
| 局部变量 | 小驼峰，无前缀 | `ItemData`、`_data` |
| 常量 | 全大写 + 下划线 | `MaxCount`、`maxCount` |
| 接口 | `I` 前缀 | `ScrollHandler`、`iScrollHandler` |
| 枚举类型 | `E` 前缀 | `ItemType`、`eItemType` |
| 事件 | `Event` 后缀 | `OnItemClick`、`itemClickEvent` |
| 委托 | `Delegate` 后缀 | `ItemClick`、`itemClickHandler` |

Unity 类型缩写对照：`Tf` `Rtf` `Go` `Img` `Btn` `Txt` `Sld` `Tgl` `Cvs` `Aud` `Anim` `Ps` `Col` `Rb`

#### ② 变量定义顺序

同一功能区块内必须按此顺序：
1. `const` 常量
2. `[SerializeField]` 序列化字段
3. `public` 属性（Properties）
4. `private` 字段
5. 事件（`event`）

#### ③ 代码结构

- 单文件是否超过 **1000 行**（超出需拆分 partial class）
- 单方法是否超过 **200 行**
- `if` 嵌套是否超过 **3 层**
- 是否存在**嵌套类定义**（class 内定义 class，禁止）
- MonoBehaviour 生命周期方法是否乱序（Awake → OnEnable → Start → Update → FixedUpdate → LateUpdate → OnDisable → OnDestroy）

#### ④ 注释规范

- 所有 `public` 方法是否有 `/// <summary>` XML 文档注释
- 所有字段、常量是否有**行内中文注释**
- `[SerializeField]`、`[Header]` 等 Attribute 是否**单独占一行**（不与声明同行）

违规示例：
```csharp
[SerializeField] float _itemHeight;   // ❌ Attribute 与声明同行
public void Init() { ... }            // ❌ 缺少 XML 注释
private int _index;                   // ❌ 缺少行内注释
```

#### ⑤ 技术栈限制

检查是否使用了以下**禁止项**：

| 违规用法 | 应替代为 |
|---------|---------|
| 运行时 LINQ（`Where`/`Select`/`FirstOrDefault` 等，非 Editor 代码） | 手写循环 |
| `StartCoroutine` / `IEnumerator` | UniTask |
| `Animator.SetTrigger/CrossFade` 控制动画过渡 | DOTween |

#### ⑥ 性能规范（仅检查格式层面可识别的）

- `Update`/`FixedUpdate`/`LateUpdate` 中是否存在 `new`（值类型除外）或 `Instantiate`
- `GetComponent`/`Find`/`FindObjectOfType` 是否在循环或 Update 中调用（未缓存）
- 字符串拼接是否在热路径使用 `+` 操作符

---

### Step 3 — 直接修改文件

- 使用编辑工具直接对文件进行修改，**不在聊天框输出修改内容**
- 所有违规项一次性修复完毕后，仅输出一行简短总结：

```
已修复 X 处格式违规（命名 A 处 / 注释 B 处 / 其他 C 处）。
```

- 若**无任何违规**，输出：

```
格式检查通过，无需修改。
```

---

## 注意事项

- 若代码来自第三方插件（如 `Assets/Plugins/` 路径），**不审查，直接跳过**
- Editor 专用脚本（位于 `Editor/` 目录或文件名含 `Editor`）中 LINQ 不视为违规
- 部分规则（如方法行数）在代码片段不完整时无法判断，需说明「无法判断，请提供完整文件」
