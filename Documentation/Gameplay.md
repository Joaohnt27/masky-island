> Histórico: moedas e timer foram retirados em 05/11/2026. Consulte Historia-Etapa-02.md para o estado atual. Os testes relatados abaixo pertencem à versão anterior.

# Gameplay: missão e mineração

Pause e painéis de fim de jogo atuais: [PauseAndEndGame.md](PauseAndEndGame.md).

## Fluxo

Splash → clique → MainMenu → nickname → vídeo existente → menu → Jogar → Cave → porta para Island → NPC (E) → terminar as dez falas → missão de 20 minutos → área à esquerda da Island → MINERAR → obter pelo menos 250 moedas.

O painel aparece dentro de uma distância de 2 unidades do objeto MiningSpot, na Island, provisoriamente em `(-3, -4, 0)`. Antes da missão, ele orienta conversar com o NPC e mantém o botão indisponível. Sair da área esconde o painel. Cada tentativa dura 3 segundos. O movimento e as outras interações ficam bloqueados durante a tentativa; o timer continua correndo. Cave não possui mineração. O posicionamento artístico final deve ser conferido no Editor.

As alterações de Splash, menu, áudio e vídeo estão detalhadas em [MenuAndSettings.md](MenuAndSettings.md).

Vitória e Game Over interrompem a missão. A última recompensa pode ultrapassar 250: o saldo real permanece visível. O timer fica congelado na vitória e em 00:00 na derrota. Tentativas canceladas não concedem moedas.

## Arquivos criados

- `Assets/Scripts/MiningSpot.cs`: área por distância, coroutine, sorteio, bloqueio, cancelamento e eventos opcionais de animação.
- `Assets/Scripts/MiningUI.cs`: painel, botão, barra e resultado, com controlador fora do painel desativável.
- `Assets/Scripts/MissionResultUI.cs`: feedback final local da cena.
- Arquivos `.meta` dos três scripts.
- Este documento.
- `Tests/GameplayValidation/GameplayValidation.cs`, `GameplayValidationRunner.cs` e `Run.ps1`: execução reproduzível dos testes em cópia isolada, fora dos assets do jogo.

## Arquivos modificados

- `GameManager.cs`: meta central de 250, início único, estados terminais exclusivos, reset e evento de estado.
- `MoneyManager.cs`: evento de saldo, validação de adições, proteção contra overflow e integração com vitória.
- `MoneyUI.cs`: atualização por eventos; esconde somente o texto, preservando o controlador ativo.
- `TimerUI.cs`: inicialização oculta, proteção contra referências ausentes, arredondamento que permite exibir 20:00 no início e persistência visual nos estados finais.
- `DialogueController.cs`: uma chamada de início por conclusão; consulta o singleton persistente; bloqueia movimento enquanto o diálogo está aberto.
- `PlayerClickMovement.cs`: bloqueios por proprietário, filtro de cliques sobre UI, validação de destinos de portas e movimento sem ultrapassar o destino.
- `NPCInteraction.cs` e `Interactable.cs`: respeitam bloqueios; removido log de distância a cada frame.
- `ScrollInteraction.cs`: bloqueia enquanto o pergaminho está aberto e libera ao fechar/desativar.
- `MainMenuController.cs`: uma nova partida pelo botão Jogar reseta a missão anterior, caso exista.
- `Assets/Scenes/Island.unity`: HUD corrigida, referências de diálogo/pergaminho, feedback final; porta inacabada sem destino desativada.
- `Assets/Scenes/Cave.unity`: gerenciadores, HUD, timer e feedback final configurados; a mineração foi transferida para Island na etapa seguinte.

## Configuração no Unity

As referências necessárias já foram salvas nas cenas. Não é necessário arrastar componentes para ativar as mecânicas.

- `GameManager`: Mission Time = 1200; Target Money = 250.
- `MoneyManager`: Starting Money = 0.
- `Player`: Tag = Player em Cave e Island. Não foi adicionado Rigidbody ou Collider ao personagem.
- `Island/MiningSpot`: Player atribuído ao PlayerClickMovement da cena; Interaction Distance = 2; Mining Duration = 3; Failure Chance = 0.
- `Island/MiningUI`: MiningSpot, MinigPanel, MinigButton, MinigProgressBar e MinigResultText atribuídos. O listener do botão é registrado pelo script; deixar o On Click do Inspector vazio para evitar duplicação.
- `MinigProgressBar`: mínimo 0, máximo 1, sem interação. Preenche durante a tentativa e reinicia quando começa a próxima.
- `MoneyUI`: MoneyText atribuído. O controlador deve continuar ativo; somente o texto é ocultado.
- `TimerUI`: TimerText atribuído nas duas cenas.
- `MissionResultUI`: painel/texto de resultado atribuídos nas duas cenas.
- `DialogueController` e `ScrollInteraction`: Player Movement atribuído.
- `Island/Door_Interactable (1)`: desativado por não ter cena de destino. Só reativar quando houver um destino definido.

Para mover a área, selecionar MiningSpot em Island e ajustar sua posição/distância; o gizmo amarelo mostra o alcance. A detecção usa distância 3D, coerente com as interações existentes. Não exige trigger nem tecla E.

Os gerenciadores existem nas duas cenas para permitir abri-las diretamente. Seus singletons eliminam duplicatas ao trocar de cena. Eles não guardam referências a Canvas, Player ou painéis locais. As interfaces removem suas inscrições de eventos ao serem desativadas.

## Recompensas

| Moedas | Peso inicial |
|---:|---:|
| 1 | 30 |
| 2 | 22 |
| 5 | 18 |
| 10 | 12 |
| 25 | 8 |
| 50 | 5 |
| 75 | 3 |
| 100 | 2 |

Os pesos somam 100, portanto correspondem às porcentagens solicitadas com Failure Chance = 0. Se os pesos forem alterados, o sorteio os normaliza pela soma. Failure Chance é uma porcentagem separada, aplicada antes do sorteio de sucesso. Por exemplo, falha de 10% deixa 90% das tentativas distribuídas entre os pesos. Valores de recompensa ou pesos não positivos são ignorados; uma tabela sem entradas válidas produz o resultado de falha.

## Assets de animação faltantes

Não há Animator no Player, Animator Controller ou Animation Clips. A imagem player_sprites.png contém poses parado/andando, e apenas player_front está importado como sprite separado. Faltam frames preparados para mineração e um clip/controller correspondente.

MiningSpot expõe On Mining Started e On Mining Finished. O segundo evento é disparado tanto na conclusão quanto no cancelamento, permitindo retornar à pose apropriada. Não há referências fictícias a parâmetros ou assets de animação. A mineração funciona sem esses eventos configurados.

## Validação

Compilação C# e referências serializadas das cenas verificadas. Os testes de execução são realizados em uma cópia isolada em `Temp/GameplayWork/ValidationProject`, sem entrar em Play Mode no Editor do projeto principal. O relatório da execução fica em `Temp/GameplayWork/ValidationProject/validation-results.txt`.

Resultado atualizado após a transferência para Island e implementação do menu:

- `dotnet build Assembly-CSharp.csproj --no-restore --verbosity quiet`: sucesso, zero erros e zero avisos.
- Cenas Splash, MainMenu, Cave e Island: IDs únicos, referências locais, relações pai/filho e proprietários dos componentes verificados.
- **57 verificações aprovadas em Play Mode na cópia isolada**, incluindo menu/preferências, diálogo, entrada/saída da área da Island, três segundos de progresso, cliques repetidos, recompensas/falha, distribuição, vitória em 265 moedas, timeout e troca de cena.
- A exceção de `UnityEditor.Search.SearchDatabase` do indexador do Editor foi registrada separadamente. Erros dos scripts de gameplay continuam sendo tratados como falhas. Resultado preservado em [MenuValidationResults.txt](MenuValidationResults.txt).

Para repetir a validação no Windows, executar `powershell -File Tests/GameplayValidation/Run.ps1` na pasta do projeto. Exige Unity instalado e licença disponível. O script copia o projeto para Temp, adiciona os dois scripts de teste somente nessa cópia e inicia Unity em batch. A primeira execução pode demorar para importar/compilar os pacotes. Não executar duas validações simultaneamente na mesma cópia.

Conferir visualmente no Editor principal: legibilidade dos textos na resolução usada pelo grupo, deslocamento por clique até a área, navegação real pelas portas e aparência dos painéis. Testes automatizados que reposicionam o Player e invocam Buttons não substituem essa verificação visual e de entrada física.

O sistema de profundidade foi preservado. Somente duas falas de orientação foram atualizadas para a localização da mineração na Island. A mudança não adiciona pacotes ou altera o Input System/Render Pipeline.

