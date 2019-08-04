# ArbitrageTriangularHFTBinance
 
 Esse projeto, serve de estudo para a viabilização da arbitragem triangular dentro da exchange Binance, utilizamos a conexão com todo os pares via sockets, os trades são realizados a market ou limit, o book sempre tem a profundidade de 20 e utilizamos a média ponderada para achar o valor correto e a viabilidade da arbitragem.
 
 A arbitragem é lucrativa, você pode querer acumular ETH, XRP, BTC, BNB ou USDT, que são os pares que tem mercado com outras criptomoedas.
 
 Para entender como funciona bastar seguir os passos, vamos supor que tenho um saldo de 0.003 BTC.
 
 1. Compro ETH com meu BTC
 2. Compro EOS com meu ETH.
 3. Vendo meus EOS por BTC
 
 No final terei 0.00301 BTC ou seja acabei lucrando pela deficiencia do mercado.
 
Configuração:
Criar um arquivo JSON na pasta "c:\bot\" com o nome "config.json", conteudo para configuracao:
initialValue é o total de BTC que você irá fazer por ordem(recomendo no minimo o equivalante a 10 dolares).
percValue valor de lucro no qual ele irá realizar a arbitragem(recomendo o superior as 3 taxas somadas no caso acima maior que 0.3, default 0.35)

{

 key : "SUA_KEY_AQUI",
 
 secret: "SUA_SECRET_AQUI",
 
 initialValue: 0.003,
 
 percValue: 0.35
 
}

*Criar uma pasta na raiz(windows) chama c:\bot\ ali ira conter os logs de acompanhamento.
