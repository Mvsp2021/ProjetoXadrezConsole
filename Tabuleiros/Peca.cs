namespace Tabuleiros
{
    abstract class Peca
    {
        public Posicao Posicao { get; set; }
        public Cor Cor { get; protected set; }
        public int QuantMovimentos { get; protected set; }
        public Tabuleiro Tabuleiro { get; protected set; }

        public Peca(Tabuleiro tabuleiro, Cor cor)
        {
            Posicao = null;
            Tabuleiro = tabuleiro;
            Cor = cor;
            QuantMovimentos = 0;
        }

        public void IncrementarQuantMovimentos()
        {
            QuantMovimentos++;
        }

        public abstract bool[,] MovimentosPossiveis();     
    }
}
