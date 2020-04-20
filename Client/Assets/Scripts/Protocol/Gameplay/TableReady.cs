using System;

[Serializable]
public class TableReady:GameCommand{
    public string user_id;
    public string game_id;
    public string occupied_cells;
    public int intact_cells;
}