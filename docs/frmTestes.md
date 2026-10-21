# Tela de consulta e conferência de boletos

O aplicativo WinForms inicia em `frmTestes`. O botão **Testes antigos**
abre o `Form1` original, que permanece inalterado.

## Uso

1. Selecione **Transação** ou **Operação**, informe o identificador completo
   com os zeros à esquerda e clique em **Buscar** ou pressione Enter.
2. Selecione uma operação na lista. São mostrados os dados gravados no ERP.
3. Clique em **Conferir pela DLL** para recalcular o título selecionado.
4. Clique em **Imprimir / Prévia** para gerar o PDF e abrir o visualizador.
   O botão de impressão do visualizador permite escolher a impressora.

Consultar e conferir não criam títulos, não consomem sequências e não
alteram informações do ERP. A consulta por transação pode retornar várias
operações; a conferência e a impressão são individuais.

A tela mostra o servidor, a porta e a base utilizados. Essas informações
vêm da configuração atual da DLL em `HKEY_CURRENT_USER\Interativo\Config`,
nas entradas `IpServer`, `PortaDB` e `DatabaseName`.

## Resultados

São comparados nosso número e dígito, código de barras e linha digitável.
Os valores originais ficam visíveis ao lado dos valores calculados.
Selecione as células e use Ctrl+C para copiar.

- **Não conferido:** os dados foram consultados, mas ainda não recalculados.
- **Confere:** o campo coincide com o cálculo da DLL.
- **Divergente:** o valor gravado difere do valor calculado.
- **Ausente:** falta uma informação necessária à comparação, incluindo o
  dígito do nosso número quando ele é utilizado pelo banco.
- **Erro no cálculo:** não foi possível montar o boleto; a mensagem abaixo
  das grades informa o motivo.

Zeros significativos são preservados. Na linha digitável, pontos e espaços
de apresentação não causam divergência. Nosso número utiliza a interpretação
específica do banco, mantendo o valor original na exibição.

Os bancos disponíveis são Banco do Brasil, Banrisul, Bradesco, Itaú, Sicredi,
Sicoob e NBC Bank, conforme os cadastros e carteiras já implementados no ERP.

## Impressão

O botão de impressão é independente do botão de conferência. Ele utiliza
as regras atuais de emissão do PDF da DLL; por exemplo, o Sicoob continua
rejeitando chaves inconsistentes. Nenhum documento é enviado automaticamente
à impressora e nenhum e-mail é enviado pela nova tela.

O aplicativo utiliza processo de 32 bits nas configurações Debug e Release,
compatível com a biblioteca nativa do visualizador PDF já utilizada no projeto.

## Serviço gerenciado

`ConferenciaBoletoService` fornece:

- `ConsultarTitulos(TipoConsultaBoleto tipo, string identificador)`;
- `Conferir(string operacao)`;
- `GerarPDF(string operacao)`, retornando os bytes do PDF.

A conferência e a impressão consultam novamente o título selecionado antes
de processá-lo. As assinaturas dos métodos exportados anteriores da DLL
permanecem preservadas.
