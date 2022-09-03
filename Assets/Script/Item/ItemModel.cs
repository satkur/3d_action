namespace Framework {
    public class ItemModel {
        public int ID { get; private set; }
        public string DispName { get; private set; }

        public ItemModel(int id, string dispName) {
            ID = id;
            DispName = dispName;
        }
    }
}
