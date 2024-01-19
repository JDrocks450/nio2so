using System.Drawing;

namespace nio2so.Formats.Terrain
{
    /// <summary>
    /// A mesh for the city terrain generated using <see cref="TSOCityGeom"/>
    /// </summary>
    public class TSOCityMesh : IDisposable
    {
        public TSOCityMesh()
        {
            UseBrushGeometry = true;
        }
        /// <summary>
        /// This mesh is marked to not use the <see cref="BrushGeometry"/> property
        /// </summary>
        /// <param name="UseBrushGeometry"></param>
        public TSOCityMesh(bool UseBrushGeometry) : this()
        {
            this.UseBrushGeometry = UseBrushGeometry;
        }

        public void InitializeCollections()
        {
            for (int i = 0; i < 256; i++)
            { // layers for each type of terrain
                if (i == 5) i = 155;
                if (i == 156) i = 255; // TOTAL LAYER
                Vertices.Add(i, new());
                Indices.Add(i, new());               
            }
        }

        public Bitmap? VertexColorAtlas { get; internal set; }
        /// <summary>
        /// Collections of Vertices grouped by their <see cref="TSOCityTerrainTypes"/> value    
        /// </summary>
        public Dictionary<int, List<GeomVertex>> Vertices { get; } = new();
        /// <summary>
        /// Collections of Vertex Indices grouped by their <see cref="TSOCityTerrainTypes"/> value    
        /// </summary>
        public Dictionary<int, List<int>> Indices { get; } = new();
        /// <summary>
        /// Groups the Vertices and Indices together by their shared <see cref="TSOCityBrush"/>
        /// </summary>
        public Dictionary<TSOCityBrush, TSOCityBrushGeometry> BrushGeometry { get; } = new();
        public bool UseBrushGeometry { get; }

        /// <summary>
        /// Maps the <see cref="GeomVertex"/> and optionally its brush to the <see cref="Vertices"/> property and <see cref="BrushGeometry"/>
        /// </summary>
        /// <param name="Layer"></param>
        /// <param name="Vertex"></param>
        public void AddVertex(int Layer, GeomVertex Vertex)
        {
            //add to overall mesh
            Vertices[Layer].Add(Vertex);
            Vertices[155].Add(Vertex); // TOTAL LAYER
            //add to brush geom if applicable
            if (!UseBrushGeometry || Vertex.Brush == default) 
                return;
            var geomLayer = BrushGeometry;
            if(geomLayer == null) 
                return;
            if (!geomLayer.TryGetValue(Vertex.Brush, out var collection))
            {
                collection = new();
                geomLayer.Add(Vertex.Brush, collection);
            }    
            collection.Vertices.Add(Vertex);            
        }
        /// <summary>
        /// Maps the <see cref="GeomVertex"/> Index and optionally its brush to the <see cref="Indices"/> property and <see cref="BrushGeometry"/>
        /// </summary>
        /// <param name="Layer"></param>
        public void AddIndices(int Layer, TSOCityBrush? Brush = default, params int[] IndexIncrements)
        {
            void addToCollection(IList<int> Collection, int BaseInd)
            {
                BaseInd += -4;
                foreach (var index in IndexIncrements)
                    Collection.Add(BaseInd + index);
            }
            //add to overall mesh
            addToCollection(Indices[Layer], Vertices[Layer].Count);
            addToCollection(Indices[155], Vertices[155].Count); // TOTAL LAYER
            //add to brush geom if applicable
            if (!UseBrushGeometry || Brush == default) return;
            var geomLayer = BrushGeometry;
            if (geomLayer == null) return;
            if (!geomLayer.TryGetValue(Brush, out var collection)) return;
            addToCollection(collection.Indices, collection.Vertices.Count);
        }

        public void Dispose()
        {
            VertexColorAtlas?.Dispose();
        }
    }
}
