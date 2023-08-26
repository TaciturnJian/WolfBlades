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
|query|用于查询目标|?|
|append|用于添加目标|+|
|remove|用于移除目标|-|
|update|用于修改目标|*|

### 登录命令 !login

使用格式：

`!login <user_name> <login_value>`

其中 `login_value` 可以是密码或登录口令，服务器会优先尝试使用token登录，否则再使用密码登录。
登录后，服务器会返回当前用户登录信息json文本，例如：

```json
{"Name":"root","DisplayName":"系统用户","TechGroup":[0],"UnitGroup":[0],"Authority":3,"Token":"token"}
```
