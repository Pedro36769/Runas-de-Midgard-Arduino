# Runas de Midgard 
É um jogo de tabuleiro estratégico com temática Viking, no qual dois a quatro jogadores assumem o papel de guerreiros que disputam poder através de runas ancestrais concedidas por um altar místico. 
O jogo integra elementos físicos (_tabuleiro, cartas e altar com sensor magnético e  LED Ring_) com um aplicativo Android desenvolvido na Unity, responsável por registrar  atributos, inventário e eventos globais. 
**O objetivo principal do jogo é ser o último Viking sobrevivente.**

Para a implementação do áudio, usaremos FMOD para garantir música e efeitos sonoros dinâmicos.

*******

### **Hardware**
O altar utiliza:
- Sensor Magnético para detectar a presença da peça.
- 3 Resistor 10kΩ
- LED Ring com 12 LEDs para indicar o tipo de runa como uma roleta.
- Resistor 470Ω
- Módulo Bluetooth para comunicação com o aplicativo. 
- Arduino NANO ou UNO escondido abaixo do altar.
- Sensor de toque

**Sensor Magnético**: (`Ky-003`)
Escolhemos um sensor digital pois precisamos detectar apenas a presença de campo magnético, não sua intensidade. O componente opera entre 4V~ e 24V~, não sendo necessário resistor. 

**LED Ring**: (`WCMCU-2812B-12`)
Cada LED, em seu brilho máximo, chega a uma corrente de 50mA. Portanto, o máximo que o LED Ring pode precisar são 0,6A. 

**Módulo Bluetooth**: (`HM10`)
Os 3 resistores de 10kΩ são necessários para dividir a tensão.

**Sensor de Toque**: (`Ttp223b`)
Atua como botão.
