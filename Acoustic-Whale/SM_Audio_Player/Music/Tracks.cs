namespace SM_Audio_Player.Music;

/*
 * Klasa przechowuje informacje na danego utwor, które później umieszczane zostają w liście
 * TrackProperties.TrackList.
 */
public class Tracks
{
    public Tracks(int id, string ti, string auth, string alb, string pa, string tim, string imageFilePath,int idByAdd  )
    {
        Id = id;
        Title = ti;
        Author = auth;
        Album = alb;
        Path = pa;
        Duration = tim;
        AlbumCoverPath = imageFilePath;
        IdByAdd = idByAdd;

    }

    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string Album { get; set; }
    public string Path { get; set; }
    public string Duration { get; set; }
    public string AlbumCoverPath { get; set; }
    public int IdByAdd { get; set; }
}