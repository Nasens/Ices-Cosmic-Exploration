# Fork 版本说明

版本记录文件：`fork-version.json`

## 字段

| 字段 | 含义 | 示例 |
|------|------|------|
| `main` | 主库 ICE 版本 | `0.0.78.15` |
| `sync` | 对照/合并主库次数（fork 第二位） | `1` => fork `0.1.x` |
| `translation` | 纯汉化发布次数（fork 第三位） | `3` => fork `0.x.3` |

## 合成版本（写入 DLL，卫月据此检测更新）

```
0.0.(main 第三位 + sync).(main 第四位 + translation)
```

示例：主库 `0.0.78.15` + fork `0.1.3` => **`0.0.79.18`**

## 递增规则

- **只改翻译**：`translation += 1`
- **对照主库合并**：更新 `main` 为新主库版本，然后 `sync += 1`

## 命令

```powershell
# 查看当前版本
.\scripts\version.ps1

# 纯汉化发布
.\scripts\version.ps1 -Mode BumpTranslation

# 合并主库后（先改 main，再 bump sync）
.\scripts\version.ps1 -Mode SetMain -MainVersion 0.0.79.0
.\scripts\version.ps1 -Mode BumpSync
```

编译时会自动读取 `fork-version.json` 并写入程序集版本。

## 一键编译打包

```powershell
.\scripts\build.ps1
```

会同步更新 `ICE-latest/` 与 `ICE-latest.zip`，并校验 zip 内 `ICE.dll` 版本与文件夹一致。
