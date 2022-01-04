using UnityEngine;

namespace myScript {
    
    public class TileRect {

        public Rect rect;
        public string id;
        public Side horizontal;
        public Side vertical;

        public string HorizontalId =>  id.Split('-')[0];
        public string VerticalId => id.Split('-')[1];

    }

    public enum Side {

        Top,
        Bottom,
        Left,
        Right

    }

    
 
}