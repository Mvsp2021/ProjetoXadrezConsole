﻿using ProjetoXadrezConsole.Tabuleiros;
using ProjetoXadrezConsole.Xadrez;
using System;
using Tabuleiros;

namespace ProjetoXadrezConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

            Tabuleiro tab = new Tabuleiro(8, 8);

            tab.ColocarPeca(new Torre(tab, Cor.preto), new Posicao(0, 0));
            tab.ColocarPeca(new Torre(tab, Cor.preto), new Posicao(1, 3));
            tab.ColocarPeca(new Rei(tab, Cor.preto), new Posicao(2, 4));
            Tela.ImprimirTabuleiro(tab);
            }
            catch (TabuleiroException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
