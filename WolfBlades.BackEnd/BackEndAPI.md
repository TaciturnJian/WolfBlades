# 指令文档

## 权限指令以 '!' 开头

### 登录

!login <user_name> <token_or_password>

|返回值|描述|
|:-|:-|
|-1|用户不存在|
|-2|密码或token错误|
|json|登录的用户信息（如果使用密码登录则包含token）|

```json
{
    Name: "user_name_for_login",
    DisplayName: "user_display_name(or nickname)",
    TechGroup: [1,2], //0未知 1机械 2电路 3嵌软 4算法 5管理
    UnitGroup: [1,2], //0未知 1英雄 2工程 3步兵 4哨兵 5无人机 6飞镖
    Authority: 1,     //0未知 1正常 2管理 3系统
    Token: ""
}
```

### 登出

!logout

|返回值|描述|
|:-|:-|
|-1|用户未登录|
|其他|成功退出|

## 复读指令以 '#' 开头，等价于 !echo

\#\<message\>

返回 message

## 查询指令以 '?' 开头，等价于 !query

一般格式  
?\<group\> \<identify\> \<value\>

### 用户查询 ?user

?user id \<value\>

|返回值|描述|
|:-|:-|
|-1|拒绝访问|
|-2|无法从字符串中解析ID|
|-3|ID不存在|
|json|查询得到的用户信息（不包含token）|

```json
{
    ID: 0
    Name: "user_name_for_login",
    DisplayName: "user_display_name(or nickname)",
    TechGroup: [1,2], //0未知 1机械 2电路 3嵌软 4算法 5管理
    UnitGroup: [1,2], //0未知 1英雄 2工程 3步兵 4哨兵 5无人机 6飞镖

    // 如果权限大于等于Admin
    Authority: 1,     //0未知 1正常 2管理 3系统

    // 如果权限大于等于System
    Token: "token"
}
```

?user name \<value\>

|返回值|描述|
|:-|:-|
|-1|拒绝访问|
|-2|Name不存在|
|json|查询得到的用户信息（不包含token）|

```json
{
    Name: "user_name_for_login",
    DisplayName: "user_display_name(or nickname)",
    TechGroup: [1,2], //0未知 1机械 2电路 3嵌软 4算法 5管理
    UnitGroup: [1,2], //0未知 1英雄 2工程 3步兵 4哨兵 5无人机 6飞镖

    // 如果权限大于等于Admin
    Authority: 1,     //0未知 1正常 2管理 3系统

    // 如果权限大于等于System
    Token: "token"
}
```

?user group \<value\>

|返回值|描述|
|:-|:-|
|-1|拒绝访问|
|-2|搜索结果为空|
|jsonArray|查询得到的用户ID列表，例如[0,1,2]|

### 单位查询 ?unit

?unit id \<value\>

|返回值|描述|
|:-|:-|
|-1|拒绝访问|
|-2|无法从字符串中解析ID|
|-3|ID不存在|
|json|查询得到的单位信息|

```json
{
    Name: "unit_name",
    DisplayName: "unit_recommend_display_name",
    UnitGroup: 0, //0未知 1英雄 2工程 3步兵 4哨兵 5无人机 6飞镖
    CurrentUserID: -1, //当前占用者的ID
    InProgressTasks: [0,1], //当前未完成的任务ID
    InChargeUsers: [0,1] //所有负责人员的ID
}
```

?unit name \<value\>

|返回值|描述|
|:-|:-|
|-1|拒绝访问|
|-2|搜索结果为空|
|jsonArray|一个ID列表，例如[0,1,2]|

?unit group \<JsonArray例如[1,2]\>
|返回值|描述|
|:-|:-|
|-1|拒绝访问|
|-2|搜索结果为空|
|jsonArray|一个ID列表，例如[0,1,2]|

### 任务查询 ?task

?task id \<value\>

|返回值|描述|
|:-|:-|
|-1|拒绝访问|
|-2|ID不存在|
|json|任务的详细信息|

```json
{
    Name: "task_name",
    Description: "pretty short description or empty",
    DocumentID: -1,         //任务文档的ID
    BindUnitID: -1,         //绑定的单位的ID
    InChargeUsers: [0,1]    //所有负责人员的ID
    Progress: 0.0           //任务进度 [0,1]
    StartTime: "yyyy-mm-dd hh:mm:ss",
    DeadLine: "yyyy-mm-dd hh:mm:ss",
    EndTime: "'-' or 'yyyy-mm-dd hh:mm:ss'"
}
```

?task name \<value\>

|返回值|描述|
|:-|:-|
|-1|拒绝访问|
|-2|搜索结果为空|
|jsonArray|符合条件的任务id列表|

?task unit_id \<value\>

|返回值|描述|
|:-|:-|
|-1|拒绝访问|
|-2|搜索结果为空|
|jsonArray|符合条件的任务id列表|

?task user_id \<value\>

|返回值|描述|
|:-|:-|
|-1|拒绝访问|
|-2|搜索结果为空|
|jsonArray|符合条件的任务id列表|

### 文档查询 ?document

?document id \<value\>

|返回值|描述|
|:-|:-|
|-1|拒绝访问|
|-2|ID不存在|
|json|文档的详细信息|

```json
{
    Title: "doc title",
    Description: "doc description",
    MarkdownBody: "",
    UploaderUserID: -1,
    UploadTime: "yyyy-mm-dd hh-mm-ss",
    RelatedTasks: [0,1],    //相关任务的ID列表
    RelatedUsers: [0,1],    //相关的用户列表
}
```

?document task_id \<value\>

|返回值|描述|
|:-|:-|
|-1|拒绝访问|
|-2|ID不存在|
|json|文档的详细信息|

### 留言查询 ?comment

?comment id \<value\>

|返回值|描述|
|:-|:-|
|-1|拒绝访问|
|-2|ID不存在|
|json|留言的详细信息|

```json
{
    BindTaskID: -1,
    MarkdownBody: "",
    UploaderUserID: -1,
    UploadTime: "yyyy-mm-dd hh-mm-ss",
}
```

?comment task_id \<value\>

|返回值|描述|
|:-|:-|
|-1|拒绝访问|
|-2|查询结果为空|
|jsonArray|留言ID列表|
