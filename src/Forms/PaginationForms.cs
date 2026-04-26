namespace PaginationForms
{
    public class PaginazioneInput
    {
        public int elemForPage { get; set; }
        public int numPag { get; set; }
        public string order { get; set; } = "asc";
        public string orderKey { get; set; } = "id";
    }

    public class PaginazioneOutput<T>
    {
        public List<T> elems { get; set; } = new List<T>();
        public int totElems { get; set; }
        public int totPags { get; set; }
    }

}
