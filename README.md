# FixOrderManager

Faça duas aplicações em C#: o OrderGenerator e o OrderAccumulator. 
As aplicações se comunicam usando o protocolo FIX (usar a lib do QuickFix disponivel em 
https://new.quickfixn.org/n/). 
O OrderGenerator cria ordens (NewOrderSingle) a cada 1 segundo com as seguintes 
propriedades: 
- Símbolo: escolhido aleatoriamente entre PETR4, VALE3 ou VIIA4. 
- Lado: escolhido aleatoriamente entre Compra ou Venda. 
- Quantidade: valor positivo inteiro aleatório menor que 100.000 
- Preço: valor positivo decimal múltiplo de 0.01 e menor que 1.000 

O OrderAccumulator recebe as ordens e calcula a exposição financeira por símbolo. 
Exposição financeira = somatório de (preço * quantidade executada) de cada ordem de 
compra - somatório de (preço * quantidade executada) de venda 
Ou seja, as ordens de compra aumentam a exposição e as de venda diminuem a exposição. 

O OrderAccumulator terá um limite interno constante R$ 1.000.000. 
Isso significa que qualquer ordem que venha a ultrapassar em valor absoluto o limite de 
exposição deve ser respondida com uma rejeição. 
Ou seja, caso a ordem seja aceita, o OrderAccumulator deve responder com um 
ExecutionReport e a ordem deve ser considerada no cálculo de exposição. 
E caso a ordem seja rejeitada, o OrderAccumulator deve responder com um OrderReject e a 
ordem não deve ser considerada no cálculo de exposição.

OrderAccumulator & OrderGenerator em execução
![image](https://user-images.githubusercontent.com/123670661/226199801-8130e8f2-9b06-4d90-9b94-8e1eaab1737a.png)


Foram usadas as seguintes aplicações de exemplo disponíveis na QuickFix Engine para o desenvolvimento das aplicações OrderAccumulator e OrderGenerator, respectivamente: ![image](https://user-images.githubusercontent.com/123670661/226200932-35c84b11-c534-4d5f-accc-80c546764cfb.png)
