# 小狼窝数据服务器

一个简单的C#后端，支持用户管理、数据增删改查。

## 第三方库

[Fleck(支持IPv4、IPv6的WebSocket库)](https://github.com/statianzo/Fleck)

[Newtonsoft.Json(一个高速且简洁的Json序列化、反序列化库)](https://github.com/JamesNK/Newtonsoft.Json)

## 服务器指令

### 命令符号(!)

`!<command>`

其中，command的可用选项如下：

|命令|说明|快捷符号|
|-|-|-|
|login|用于当前连接的登录，未登录的连接将无法使用其他命令||
|echo|用于回显数据，测试发送和接受的数据是否一致|#|
|query|用于查询目标，需要提供选择器和值|?|
|append|用于添加目标，需要提供详细信息|+|
|remove|用于移除目标，需要提供id|-|
|update|用于修改目标，需要提供id和值|*|

### 登录命令 !login

使用格式：

`!login <user_name> <login_value>`

其中 `login_value` 可以是密码或登录口令，服务器会优先尝试使用token登录，否则再使用密码登录。
登录后，服务器会返回当前用户登录信息json文本，例如：

```json
{"Name":"root","DisplayName":"系统用户","TechGroup":[0],"UnitGroup":[0],"Authority":3,"Token":"token"}
```

### 回显命令 !echo (#)

使用格式：

`!echo <message>`

或快捷指令：

`#<message>`

服务器会重复 `<message>` 表示的内容，并返回给客户端一次（目前不支持通过echo查询的功能）

### 查询命令 !query (?)

使用格式：

`!query <target> <selector> <value>`

或快捷指令：

`?<target> <selector> <value>`

### 添加命令 !append (+)

使用格式：

`!append <target> <content>`

或快捷指令：

`+<target> <content>`

### 移除命令 !remove (-)

使用格式：

`!remove <target> <id>`

或快捷指令：

`-<target> <id>`

### 更新命令 !update (*)

使用格式：

`!update <target> <id> <content>`

或快捷指令：

`*<target> <id> <content>`
