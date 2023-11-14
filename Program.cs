using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

class ContaBancaria
{
    public int Numero { get; set; }
    public string Titular { get; set; }
    public double Saldo { get; set; }
}

class Program
{
    static string connectionString = "Data Source=contas.db";
    static HashSet<ContaBancaria> contas = new HashSet<ContaBancaria>();

    static void Main()
    {
        // Cria o banco de dados e a tabela se não existirem
        CriarBancoETabela();

        // Menu principal
        while (true)
        {
            Console.WriteLine("1. Criar Conta");
            Console.WriteLine("2. Depositar");
            Console.WriteLine("3. Sacar");
            Console.WriteLine("4. Verificar Saldo");
            Console.WriteLine("5. Listar Contas");
            Console.WriteLine("0. Sair");
            Console.Write("Escolha uma opção: ");

            string escolha = Console.ReadLine();

            switch (escolha)
            {
                case "1":
                    CriarConta();
                    break;
                case "2":
                    Depositar();
                    break;
                case "3":
                    Sacar();
                    break;
                case "4":
                    VerificarSaldo();
                    break;
                case "5":
                    ListarContas();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Opção inválida. Tente novamente.");
                    break;
            }
        }
    }

    static void CriarBancoETabela()
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS Contas (Numero INTEGER PRIMARY KEY, Titular TEXT, Saldo REAL)";
                command.ExecuteNonQuery();
            }
        }
    }

    static void CriarConta()
    {
        ContaBancaria novaConta;
        do
        {
            novaConta = new ContaBancaria
            {
                Numero = new Random().Next(1000, 2000),
                Titular = ObterInput("Titular da conta: "),
                Saldo = 0
            };
        } while (!contas.Add(novaConta));  // Adiciona a nova conta ao HashSet e repete se já existir

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Contas (Numero, Titular, Saldo) VALUES ($Numero, $Titular, $Saldo)";
                command.Parameters.AddWithValue("$Numero", novaConta.Numero);
                command.Parameters.AddWithValue("$Titular", novaConta.Titular);
                command.Parameters.AddWithValue("$Saldo", novaConta.Saldo);

                command.ExecuteNonQuery();
            }
        }

        Console.WriteLine($"Conta criada com sucesso. Número da Conta: {novaConta.Numero}");
    }

    static void Depositar()
    {
        int numeroConta = ObterNumeroConta();

        double valor = ObterValor("Digite o valor a ser depositado: ");

        ContaBancaria conta = null;

        foreach (var c in contas)
        {
            if (c.Numero == numeroConta)
            {
                conta = c;
                break;
            }
        }

        if (conta != null)
        {
            conta.Saldo += valor;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Contas SET Saldo = Saldo + $Valor WHERE Numero = $Numero";
                    command.Parameters.AddWithValue("$Valor", valor);
                    command.Parameters.AddWithValue("$Numero", numeroConta);

                    command.ExecuteNonQuery();
                }
            }

            Console.WriteLine("Depósito realizado com sucesso.");
        }
        else
        {
            Console.WriteLine("Conta não encontrada. Verifique o número da conta e tente novamente.");
        }
    }

    static void Sacar()
    {
        int numeroConta = ObterNumeroConta();

        double valor = ObterValor("Digite o valor a ser sacado: ");

        ContaBancaria conta = null;

        foreach (var c in contas)
        {
            if (c.Numero == numeroConta)
            {
                conta = c;
                break;
            }
        }

        if (conta != null && conta.Saldo >= valor)
        {
            conta.Saldo -= valor;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Contas SET Saldo = Saldo - $Valor WHERE Numero = $Numero AND Saldo >= $Valor";
                    command.Parameters.AddWithValue("$Valor", valor);
                    command.Parameters.AddWithValue("$Numero", numeroConta);

                    command.ExecuteNonQuery();
                }
            }

            Console.WriteLine("Saque realizado com sucesso.");
        }
        else
        {
            Console.WriteLine("Saque não permitido. Verifique o número da conta ou o saldo disponível.");
        }
    }

    static void VerificarSaldo()
    {
        int numeroConta = ObterNumeroConta();

        ContaBancaria conta = null;

        foreach (var c in contas)
        {
            if (c.Numero == numeroConta)
            {
                conta = c;
                break;
            }
        }

        if (conta != null)
        {
            Console.WriteLine($"Saldo atual da conta {numeroConta}: R$ {conta.Saldo}");
        }
        else
        {
            Console.WriteLine("Conta não encontrada. Verifique o número da conta e tente novamente.");
        }
    }

    static void ListarContas()
    {
        Console.WriteLine("Contas cadastradas:");
        foreach (var conta in contas)
        {
            Console.WriteLine($"Número: {conta.Numero}, Titular: {conta.Titular}, Saldo: R$ {conta.Saldo}");
        }
    }

    static int ObterNumeroConta()
    {
        return ObterInputInt("Digite o número da conta: ");
    }

    static double ObterValor(string mensagem)
    {
        return ObterInputDouble(mensagem);
    }

    static string ObterInput(string mensagem)
    {
        Console.Write(mensagem);
        return Console.ReadLine();
    }

    static int ObterInputInt(string mensagem)
    {
        int valor;
        while (!int.TryParse(ObterInput(mensagem), out valor))
        {
            Console.WriteLine("Valor inválido. Tente novamente.");
        }
        return valor;
    }

    static double ObterInputDouble(string mensagem)
    {
        double valor;
        while (!double.TryParse(ObterInput(mensagem), out valor))
        {
            Console.WriteLine("Valor inválido. Tente novamente.");
        }
        return valor;
    }
}
