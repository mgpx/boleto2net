# Sicoob no fluxo do mobile

O banco 756 utiliza a carteira `1/01` já implementada na biblioteca.
Os pontos de entrada existentes da DLL continuam disponíveis:

- `GerarInfoBoleto`: calcula os dados de um novo boleto a partir da integração bancária do ERP.
- `GerarPDF`, `GerarPDFOperacao` e `GerarPDFTransacao`: geram o PDF de títulos existentes.

## Cadastro da integração

Os códigos abaixo correspondem a `intbcodet.itbd_codigo`.

| Código | Informação utilizada |
| --- | --- |
| 001 | CPF/CNPJ do beneficiário, somente números |
| 002 / 003 | Agência de até quatro dígitos e seu dígito |
| 004 | Código do beneficiário de seis dígitos seguido do dígito verificador |
| 005 | Nome do beneficiário |
| 009 / 010 | Carteira `1` e modalidade `01` |
| 012 / 013 | Tipo de impressão e distribuição do boleto |
| 016 / 017 | Espécie do título e aceite (`A` ou `N`) |
| 024 | Local de pagamento; quando vazio, utiliza o texto padrão da biblioteca |
| 025 / 026 | Conta do boleto de até oito dígitos e seu dígito |
| 101 a 106 | Instruções impressas, na ordem do cadastro, omitindo textos vazios |

O convênio `0815713`, por exemplo, corresponde ao beneficiário `081571`
com dígito `3`. Os zeros à esquerda são preservados. A conta do boleto
vem de `025/026`; os campos `027/028` identificam a conta corrente.

O código `0` do parâmetro `011` não é convertido diretamente para o enum
da biblioteca. Seguindo o fluxo existente, `itbc_tpproc = 001` representa
emissão sem registro; os demais tipos utilizam emissão com registro.
Esta alteração não gera remessa nem registra cobranças no banco.

## Novos boletos

É utilizada a sequência de `intbco.itbc_cont_individual`, sem sufixo anual
para o Sicoob. Configuração, vencimento e valor são validados antes de
solicitar o próximo número. A sequência deve produzir valores entre `1`
e `9999999`; um valor fora desse intervalo é rejeitado, sem reiniciar
ou alterar a sequência automaticamente.

O contrato de retorno de `GerarInfoBoleto` é mantido:

`Código de barras;Linha digitável;Nosso número;Dígito;Nosso número formatado`

O nosso número retornado contém sete dígitos sem o verificador, que é
retornado separadamente. A parcela é `1`, conforme a assinatura atual.
A geração dos dados não grava o título financeiro.

## PDF de títulos existentes

O ERP deve fornecer `pfin_nossonumero` com oito posições, incluindo o
dígito: `00000204` representa `0000020-4`. A parcela é lida de
`pfin_parcela` e deve estar entre `1` e `999`.

Antes de gravar o PDF, são conferidos o dígito do nosso número, o código
de barras e a linha digitável em relação ao cadastro do ERP. Na linha
digitável são aceitos pontos e espaços de formatação. Ausência ou
divergência dessas informações interrompe a geração com uma mensagem
específica. Os métodos de exportação mantêm o tratamento de erros existente.

As instruções `101` a `106` são utilizadas como cadastradas, incluindo
as mensagens de mora e multa. Não são acrescentadas mensagens automáticas
de encargos ao boleto do Sicoob. O endereço do beneficiário continua vindo
do cadastro da empresa, como nos demais bancos.

## Validação

Os testes `SicoobInterativoTests` cobrem os dois fluxos sem conexão ao banco
real: a geração de novos dados recebe uma sequência controlada, e o PDF
recebe entidades de teste. Há conferência de seis códigos de barras
emitidos pelo ERP, parcela `2`, chaves divergentes, configuração inválida,
limites da sequência e compatibilidade com o fluxo do Sicredi.

Não são necessárias alterações de estrutura no banco de dados.
