# Projeto RetrievingPrimaryKeys

- [Introdução](#introdução)
- [Diferentes tipos de chaves primárias](#diferentes-tipos-de-chaves-primárias)
- [Exemplo na prática](#exemplo-na-prática)
- [Sumário](#sumário)

# Introdução

Depois de uma pincelada rápida no [HelloWorld](https://github.com/prrandrade/OrleansStudy/tree/master/study/01-HelloWorld), vamos ver com mais calma como trabalhamos com chaves primárias dentro dos **Grains** - afinal de contas, as chaves primárias não servem apenas para identificar os **Grains** na lógica do Orleans - a própria informação pode ser usada dentro da lógica de negócio, claro!

# Diferentes tipos de chaves primárias

Neste exemplo, temos cinco **Grains**, sendo que cada um implementa um tipo diferente de chave primária. A ideia é resgatar a chave primária (e a secundária em caso de chave composta), para que ambas possam ser usadas na lógica de negócio.

Diretamente ao ponto, temos três tipos de Grain com chave primária:

- `IGrainWithGuidKey`, onde a chave primária é um `Guid`. Para resgatar o valor da chave primária, usamos o método `this.GetPrimaryKey()`.

- `IGrainWithIntegerKey`, onde a chave primária é um `Long` (sim, o nome engana). Para resgatar o valor da chave primária, usamos o método `this.GetPrimaryKeyLong()`.

- `IGrainWithStringKey`, onde a chave primária é um `String`. Para resgar o valor da chave primária, usamos o método `this.GetPrimaryKeyString()`.

Existem também dois tipos de **Grains** com chave composta - formada por uma chave primária e secundária, sendo que ambas devem ser preenchidas quando o **Grain** é ativado pelo **Client**:

- `IGrainWithGuidCompoundKey`, onde a chave primária é um `Guid` e a chave secundária é um `String`. Para resgatar os valores, usamos o método `this.GetPrimaryKey(out var keyExt)`, que devolve o valor da chave primária (`Guid`) e preenche o valor da chave secundária (`String`) na variável `keyExt` (recurso do C# 7.0, diga-se). Se você só precisa da chave primária, a chamada do método deve ser `this.GetPrimaryKey(out _)`.

- `IGrainWithIntegerCompoundKey`, onde a chave primária é um `Long` (novamente o nome engana) e a chave secundária é um `String`. Para resgatar os valores, usamos o método `this.GetPrimaryKeyLong(out var keyExt)`, que devolve o valor da chave primária (`Long`) e preenche o valor da chave secundária (`String`) na variável `keyExt`. Se você só precisa da chave primária, a chamada do método deve ser `this.GetPrimaryKeyLong(out _)`.

# Exemplo na prática

O código deste exemplo é bem simples, apenas ativando os **Grains** usando as respectivas chaves primárias (e secundárias, quando necessário) e usando métodos que, por sua vez, devolvem as mesmas chaves usadas na ativação, sem mistério.

# Sumário

De forma resumida:

- Chaves primárias podem (e devem) ser usadas para individualizar a regra de negócio, elas não precisam ser usadas apenas para a ativação dos **Grains**.

- Existem cinco tipos diferentes de chaves primárias no Orleans para individualizar **Grains**.

- No caso de chaves compostas, não é possível nem ativar **Grains** sem ambos os componentes da chave e nem recuperar apenas um componente da chave primária (o que conseguimos fazer é ignorar o segundo componente de uma chave composta graças ao C# 7.0).



