namespace API.Helpers
{
    public class PaginationParams
    {
        private const int MaxPageSize = 50; //nº max de items por pagina
        public int PageNumber { get; set; } = 1;
        private int _pageSize = 10; //retorna 10 items por pagina
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value; /*verifica se a quantidade de items q client pediu é maior q MaxPageSize.
                                                                             se client pedir 200items(value) retorna 50(MaxPageSize), se for 40(value) retorna 40(value)*/
        }
    }
}
