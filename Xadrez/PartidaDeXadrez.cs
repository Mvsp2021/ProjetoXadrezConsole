﻿using ProjetoXadrezConsole.Tabuleiros;
using ProjetoXadrezConsole.Xadrez;
using System.Collections.Generic;
using Tabuleiros;

namespace Xadrez
{
    class PartidaDeXadrez
    {
        public Tabuleiro Tab { get; private set; }

        public bool Terminada { get; private set; }

        public int Turno { get; private set; }

        public Cor JogadorAtual { get; private set; }

        private HashSet<Peca> pecas;

        private HashSet<Peca> capturadas;

        public bool Xeque { get; private set; }

        public Peca VulneravelEnPassant { get; private set; }

        public PartidaDeXadrez()
        {
            Tab = new Tabuleiro(8, 8);
            Turno = 1;
            JogadorAtual = Cor.branco;
            Terminada = false;
            Xeque = false;
            VulneravelEnPassant = null;
            pecas = new HashSet<Peca>();
            capturadas = new HashSet<Peca>();
            ColocarPecas();
        }

        public Peca ExecutarMovimento(Posicao origem, Posicao destino)
        {
            Peca p = Tab.RetirarPeca(origem);
            p.IncrementarQuantMovimentos();
            Peca pecaCapturada = Tab.RetirarPeca(destino);
            Tab.ColocarPeca(p, destino);
            if (pecaCapturada != null)
            {
                capturadas.Add(pecaCapturada);
            }

            // #Jogada Especial Roque Pequeno
            if (p is Rei && destino.Coluna == origem.Coluna + 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna + 3);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna + 1);
                Peca t = Tab.RetirarPeca(origemT);
                t.IncrementarQuantMovimentos();
                Tab.ColocarPeca(t, destinoT);
            }

            // #Jogada Especial Roque Grande
            if (p is Rei && destino.Coluna == origem.Coluna - 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna - 4);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna - 1);
                Peca t = Tab.RetirarPeca(origemT);
                t.IncrementarQuantMovimentos();
                Tab.ColocarPeca(t, destinoT);
            }

            // #JogadaEspecial En Passant
            if (p is Peao)
            {
                if (origem.Coluna != destino.Coluna && pecaCapturada == null)
                {
                    Posicao posP;
                    if (p.Cor == Cor.branco)
                    {
                        posP = new Posicao(destino.Linha + 1, destino.Coluna);
                    }
                    else
                    {
                        posP = new Posicao(destino.Linha - 1, destino.Coluna);
                    }
                    pecaCapturada = Tab.RetirarPeca(posP);
                    capturadas.Add(pecaCapturada);
                }
            }

            return pecaCapturada;
        }

        public void DesfazMovimento(Posicao origem, Posicao destino, Peca pecaCapturada)
        {
            Peca p = Tab.RetirarPeca(destino);
            p.DecrementarQuantMovimentos();
            if (pecaCapturada != null)
            {
                Tab.ColocarPeca(pecaCapturada, destino);
                capturadas.Remove(pecaCapturada);
            }
            Tab.ColocarPeca(p, origem);

            // #Jogada Especial Roque Pequeno
            if (p is Rei && destino.Coluna == origem.Coluna + 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna + 3);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna + 1);
                Peca t = Tab.RetirarPeca(destinoT);
                t.DecrementarQuantMovimentos();
                Tab.ColocarPeca(t, origemT);
            }

            // #Jogada Especial Roque Grande
            if (p is Rei && destino.Coluna == origem.Coluna - 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna - 4);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna - 1);
                Peca t = Tab.RetirarPeca(destinoT);
                t.DecrementarQuantMovimentos();
                Tab.ColocarPeca(t, origemT);
            }

            // #Jogada Especial En Passant
            if (p is Peao)
            {
                if (origem.Coluna != destino.Coluna && pecaCapturada == VulneravelEnPassant)
                {
                    Peca peao = Tab.RetirarPeca(destino);
                    Posicao posP;
                    if (p.Cor == Cor.branco)
                    {
                        posP = new Posicao(3, destino.Coluna);
                    }
                    else
                    {
                        posP = new Posicao(4, destino.Coluna);
                    }
                    Tab.ColocarPeca(peao, posP);
                }
            }

        }

        public void RealizaJogada(Posicao origem, Posicao destino)
        {
            Peca pecaCapturada = ExecutarMovimento(origem, destino);
            if (EstaEmXeque(JogadorAtual))
            {
                DesfazMovimento(origem, destino, pecaCapturada);
                throw new TabuleiroException("Você não pode se colocar em xeque");
            }

            Peca p = Tab.peca(destino);

            // #Jogada Especial Promoção
            if (p is Peao)
            {
                if ((p.Cor == Cor.branco && destino.Linha == 0) ||  (p.Cor == Cor.preto && destino.Linha == 7))
                {
                    p = Tab.RetirarPeca(destino);
                    pecas.Remove(p);
                    Peca dama = new Dama(Tab, p.Cor);
                    Tab.ColocarPeca(dama, destino);
                    pecas.Add(dama);
                }
            }

            if (EstaEmXeque(Adversaria(JogadorAtual)))
            {
                Xeque = true;
            }
            else
            {
                Xeque = false;
            }

            if (TesteXequeMate(Adversaria(JogadorAtual)))
            {
                Terminada = true;
            }
            else
            {
                Turno++;
                MudaJogador();
            }

            //# Jogada Especial En Passant
            if (p is Peao && destino.Linha == origem.Linha - 2 || destino.Linha == origem.Linha + 2)
            {
                VulneravelEnPassant = p;
            }
            else
            {
                VulneravelEnPassant = null;
            }
        }

        public void ValidarPosicaoDeOrigem(Posicao pos)
        {
            if (Tab.peca(pos) == null)
            {
                throw new TabuleiroException("Não existe peça na posição de origem escolhida!");
            }
            if (JogadorAtual != Tab.peca(pos).Cor)
            {
                throw new TabuleiroException("A peça de origem não é sua!");
            }
            if (!Tab.peca(pos).ExisteMovimentosPossiveis())
            {
                throw new TabuleiroException("Não há movimentos possíveis para a peça de origem escolhida!");
            }
        }

        public void ValidarPosicaoDeDestino(Posicao origem, Posicao destino)
        {
            if (!Tab.peca(origem).MovimentoPossivel(destino))
            {
                throw new TabuleiroException("Posição de destino inválida!");
            }
        }

        private void MudaJogador()
        {
            if (JogadorAtual == Cor.branco)
            {
                JogadorAtual = Cor.preto;
            }
            else
            {
                JogadorAtual = Cor.branco;
            }
        }

        public HashSet<Peca> pecasCapturadas(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in capturadas)
            {
                if (x.Cor == cor)
                {
                    aux.Add(x);
                }
            }
            return aux;
        }

        public HashSet<Peca> PecasEmJogo(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in pecas)
            {
                if (x.Cor == cor)
                {
                    aux.Add(x);
                }
            }
            aux.ExceptWith(pecasCapturadas(cor));
            return aux;
        }

        private Cor Adversaria(Cor cor)
        {
            if (cor == Cor.branco)
            {
                return Cor.preto;
            }
            else
            {
                return Cor.branco;
            }
        }

        private Peca Rei(Cor cor)
        {
            foreach (Peca x in PecasEmJogo(cor))
            {
                if (x is Rei)
                {
                    return x;
                }
            }
            return null;
        }

        public bool EstaEmXeque(Cor cor)
        {
            Peca r = Rei(cor);
            if (r == null)
            {
                throw new TabuleiroException("Não tem rei da cor " + cor + " no tabuleiro!");
            }
            foreach (Peca x in PecasEmJogo(Adversaria(cor)))
            {
                bool[,] matriz = x.MovimentosPossiveis();
                if (matriz[r.Posicao.Linha, r.Posicao.Coluna])
                {
                    return true;
                }
            }
            return false;
        }

        public bool TesteXequeMate(Cor cor)
        {
            if (!EstaEmXeque(cor))
            {
                return false;
            }
            foreach (Peca x in PecasEmJogo(cor))
            {
                bool[,] matriz = x.MovimentosPossiveis();
                for (int i = 0; i < Tab.Linhas; i++)
                {
                    for (int j = 0; j < Tab.Colunas; j++)
                    {
                        if (matriz[i, j])
                        {
                            Posicao origem = x.Posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCapturada = ExecutarMovimento(origem, destino);
                            bool testeXeque = EstaEmXeque(cor);
                            DesfazMovimento(origem, destino, pecaCapturada);
                            if (!testeXeque)
                            {
                                return false;
                            }
                        }
                    }

                }
            }
            return true;
        }

        public void ColocarNovaPeca(char coluna, int linha, Peca peca)
        {
            Tab.ColocarPeca(peca, new PosicaoXadrez(coluna, linha).ToPosicao());
            pecas.Add(peca);
        }

        public void ColocarPecas()
        {
            ColocarNovaPeca('a', 1, new Torre(Tab, Cor.branco));
            ColocarNovaPeca('b', 1, new Cavalo(Tab, Cor.branco));
            ColocarNovaPeca('c', 1, new Bispo(Tab, Cor.branco));
            ColocarNovaPeca('d', 1, new Dama(Tab, Cor.branco));
            ColocarNovaPeca('e', 1, new Rei(Tab, Cor.branco, this));
            ColocarNovaPeca('f', 1, new Bispo(Tab, Cor.branco));
            ColocarNovaPeca('g', 1, new Cavalo(Tab, Cor.branco));
            ColocarNovaPeca('h', 1, new Torre(Tab, Cor.branco));
            ColocarNovaPeca('a', 2, new Peao(Tab, Cor.branco, this));
            ColocarNovaPeca('b', 2, new Peao(Tab, Cor.branco, this));
            ColocarNovaPeca('c', 2, new Peao(Tab, Cor.branco, this));
            ColocarNovaPeca('d', 2, new Peao(Tab, Cor.branco, this));
            ColocarNovaPeca('e', 2, new Peao(Tab, Cor.branco, this));
            ColocarNovaPeca('f', 2, new Peao(Tab, Cor.branco, this));
            ColocarNovaPeca('g', 2, new Peao(Tab, Cor.branco, this));
            ColocarNovaPeca('h', 2, new Peao(Tab, Cor.branco, this));

            ColocarNovaPeca('a', 8, new Torre(Tab, Cor.preto));
            ColocarNovaPeca('b', 8, new Cavalo(Tab, Cor.preto));
            ColocarNovaPeca('c', 8, new Bispo(Tab, Cor.preto));
            ColocarNovaPeca('d', 8, new Dama(Tab, Cor.preto));
            ColocarNovaPeca('e', 8, new Rei(Tab, Cor.preto, this));
            ColocarNovaPeca('f', 8, new Bispo(Tab, Cor.preto));
            ColocarNovaPeca('g', 8, new Cavalo(Tab, Cor.preto));
            ColocarNovaPeca('h', 8, new Torre(Tab, Cor.preto));
            ColocarNovaPeca('a', 7, new Peao(Tab, Cor.preto, this));
            ColocarNovaPeca('b', 7, new Peao(Tab, Cor.preto, this));
            ColocarNovaPeca('c', 7, new Peao(Tab, Cor.preto, this));
            ColocarNovaPeca('d', 7, new Peao(Tab, Cor.preto, this));
            ColocarNovaPeca('e', 7, new Peao(Tab, Cor.preto, this));
            ColocarNovaPeca('f', 7, new Peao(Tab, Cor.preto, this));
            ColocarNovaPeca('g', 7, new Peao(Tab, Cor.preto, this));
            ColocarNovaPeca('h', 7, new Peao(Tab, Cor.preto, this));
        }
    }
}
