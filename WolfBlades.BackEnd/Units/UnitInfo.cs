namespace WolfBlades.BackEnd.Units
{
    public struct UnitInfo
    {
        public string Name = "unit_name";
        public string DisplayName = "unit_recommend_display_name";
        public int UnitGroup = 0;                                   //0未知 1英雄 2工程 3步兵 4哨兵 5无人机 6飞镖
        public int CurrentUserID = -1;                              //当前占用者的ID
        public int[] InProgressTasks = Array.Empty<int>();          //当前未完成的任务ID
        public int[] InChargeUsers = Array.Empty<int>();            //所有负责人员的ID

        public UnitInfo()
        {
        }
    }
}
