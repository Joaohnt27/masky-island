# Porta de saída da casa

A saída da casa para o centro agora reutiliza o mesmo componente AnimatedDoor das entradas externas. A porta começa fechada, abre ao passar o cursor, fecha ao retirar e permanece aberta durante a aproximação após o clique. A troca para Island aguarda a chegada ao acesso e a abertura completa.

A nova folha mostra a porta vista de dentro, revelando luz natural, um caminho costeiro e o mar. O vão e a moldura são opacos para cobrir a porta parcialmente aberta que já existia na pintura. A lanterna, o barril e os demais objetos do cenário são preservados.

## Arquivos

- Criados: `Assets/Art/Doors/HouseExit-v1.png` e `.meta`, `Tests/UIAuthoring/AuthorHouseExit.cs` e `Tests/GameplayValidation/HouseExitValidation.cs`.
- Modificado: `Assets/Scenes/House.unity`, com o sprite animado e a referência no acesso ao centro.
- Modificado: `Tests/UIAuthoring/AuthorHouseDoor.cs`, extraindo a importação da folha para um método compartilhado pelos dois utilitários de autoria.
- Nenhum script de gameplay foi alterado nesta etapa. Sons, pausa, bloqueios de interação e cancelamento por WASD reutilizam o sistema existente.

## Configuração

Nenhuma configuração manual necessária. Os cinco quadros, o colisor e as referências foram configurados no utilitário de autoria. O destino continua sendo Island e o ponto de aproximação original foi mantido. Nenhum asset faltante ou pacote novo.

## Validação

15 verificações aprovadas no Unity 6000.6.0f1: estado inicial, abertura por hover, hover sem deslocamento, sons, fechamento, bloqueios, pausa e retomada, aproximação por clique, cancelamento por WASD, espera da abertura, saída para Island, preservação das animações, retorno pela porta externa e manutenção das interações da casa. Compilação sem erros. A exceção preexistente do índice de busca do Editor é filtrada apenas quando originada em UnityEditor.Search.SearchDatabase.

Capturas aberta e fechada revisadas visualmente. Evidências em `Documentation/HouseExit`. A cena e o PNG com metadados foram conferidos por hash contra a cópia testada. A cena anterior está em `Builds/HouseExitBackup/House.unity`.

## Arte

Ajuste posterior de disposição: a peça recebeu uma orientação de plano lateral (rotação composta X 12 graus e Y 30 graus), estreitando a projeção horizontal e acompanhando a parede esquerda com os batentes verticais. A área de clique acompanha os novos limites visuais. A legenda Centro foi deslocada para o chão ao lado da soleira, com indicação para a esquerda. Ajustes em `AuthorHouseExit.cs` e `House.unity`; nenhum novo asset de imagem ou script de gameplay. As 15 verificações existentes passaram novamente, e as duas capturas foram revisadas. Evidências em `Documentation/HouseExitPerspective`; cena anterior em `Builds/HouseExitPerspectiveBackup`.

Gerada pela ferramenta integrada de imagens com transparência, usando `Assets/Art/Backgrounds/Story/02-casa.png` como referência. A pintura original não foi sobrescrita.

Prompt:

Create a production-ready 5-frame sprite sheet of the LEFT EXIT DOOR viewed FROM INSIDE the stone cottage in the attached reference image. Match its warm shaded hand-painted ink-and-watercolor art, chunky irregular gray-ochre stone arch with a little moss, weathered vertical brown wooden planks, two dark iron straps and iron ring handle. Exact grid ONE ROW of FIVE equal cells, identical stone arch shape/scale/baseline in all five frames. Full arched doorway, narrow stone jambs and ONE low stone threshold per cell. Frame 1 closed. Frame 2 quarter open. Frame 3 half open. Frame 4 three quarters open. Frame 5 fully open, wooden leaf narrow against LEFT jamb. Door swings inward on LEFT hinges with convincing frame-by-frame perspective, NOT fading. Doorway seen nearly frontally with the slight angle matching the left-hand doorway in the reference. In the uncovered opening, show an OPAQUE small painted view of daylight outside: sandy path and a little coastal grass at bottom, muted blue sea and pale blue-gray sky above. Same outdoor view fixed in every frame, increasingly revealed as door opens. This is viewed from indoors looking OUT, never a dark amber room. Inside all parts of the arch aperture must be fully opaque, so the patch can cover the original partially open door in the background. Outside the complete stone arch silhouette must be genuinely TRANSPARENT. No surrounding wall, no lantern, no roof, no barrel, no furniture, no extended floor, no exterior shadows, no text or grid. Keep the full door and arch within each cell, with generous transparent margin; no part crosses between cells. Arch tall and relatively slim, about 0.58 width-to-height ratio. Preserve the quiet coastal game style.

