# Runas de Midgard 
É um jogo de tabuleiro estratégico com temática Viking, no qual dois a quatro jogadores assumem o papel de guerreiros que disputam poder através de runas ancestrais concedidas por um altar místico. 
O jogo integra elementos físicos (_tabuleiro, cartas e altar com sensor magnético e  LED Ring_) com um aplicativo Android desenvolvido na Unity, responsável por registrar  atributos, inventário e eventos globais. 
**O objetivo principal do jogo é ser o último Viking sobrevivente.**

Para a implementação do áudio, usaremos FMOD para garantir música e efeitos sonoros dinâmicos.

*******

### **Hardware**
O altar utiliza:
- Sensor Magnético para detectar a presença da peça.
- Resistor 10kΩ
- LED Ring com 12 LEDs para indicar o tipo de runa como uma roleta.
- Resistor 470Ω
- Módulo Bluetooth para comunicação com o aplicativo. 
- 3 resistores 1kΩ
- Arduino NANO ou UNO escondido abaixo do altar.
- Sensor de toque

**Sensor Magnético**: (_Ky-003_)
Escolhemos um sensor digital pois precisamos detectar apenas a presença de campo magnético, não sua intensidade. O componente opera entre 4V~ e 24V~,não sendo necessário resistor. 

**LED Ring**: (_WCMCU-2812B-12_)
Cada LED, em seu brilho máximo, chega a uma corrente de 50mA. Portanto, o máximo que o LED Ring pode precisar são 0,6A. 
Colocaremos um resistor 470Ω entre o pino 7 e a entrada DI para evitar que picos de tensão danifiquem o componente.