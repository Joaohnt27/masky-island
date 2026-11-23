> Histórico: moedas e timer foram retirados em 05/11/2026. Consulte Historia-Etapa-02.md para o estado atual. Os testes relatados abaixo pertencem à versão anterior.

# Pause e fim de jogo em Cave e Island

## Estado e gameplay

O GameManager continua sendo a única fonte de estado da missão. IsPaused foi acrescentado ao estado existente; BlocksGameplay reúne pausa, vitória e derrota. O evento OnMissionStateChanged atualiza as interfaces também ao pausar/continuar. Vitória exige pelo menos 250 moedas e impede Game Over posterior; derrota mantém o timer em zero e impede vitória posterior.

ESC abre/fecha o Pause nas duas cenas jogáveis, inclusive antes da missão. O PauseController só controla input e painéis locais. Usa Keyboard.current do New Input System, sem criar outro InputAction. Não há PauseController em Splash, MainMenu ou Credits; o GameManager também rejeita pedidos de pausa fora de Cave/Island, durante uma transição ou após o fim da missão.

Pausar define Time.timeScale = 0; continuar restaura 1. Timer, movimento e animações escaladas congelam. A coroutine existente de mineração mantém seu progresso e seu bloqueio de movimento, sem cancelar nem conceder recompensa durante a pausa. Depois de continuar, termina o tempo restante. Entradas de movimento, interações e avanço de diálogo também verificam o bloqueio de gameplay. O vídeo pertence ao MainMenu e não é afetado por um menu de pausa de gameplay.

## Configurações

Pause → Configurações reutiliza SettingsMenuUI e SettingsManager. A UI salva foi copiada da configuração atual do menu, com referências locais em cada cena; volumes, vídeo e persistência continuam centralizados no mesmo sistema. Não existem novos gerenciadores de preferências.

Voltar retorna ao Pause e mantém Time.timeScale zero. ESC fecha primeiro um dropdown aberto; depois volta das configurações ao Pause; outro ESC continua a partida. O input de ESC é tratado uma única vez pelo PauseController nesse contexto.

## Fim de jogo e saídas

MissionResultUI foi ampliado, preservando o componente existente. O título anterior agora é EndGameTitle, dentro do painel completo MissionResultPanel.

- Vitória: “MISSÃO CUMPRIDA!”, mensagem sobre manter a casa e total real de moedas, inclusive acima de 250.
- Derrota: “GAME OVER”, mensagem sobre o tempo esgotado e total de moedas.
- Nos dois casos: timer parado, mineração indisponível, movimento/interações bloqueados, Pause indisponível.
- Recomeçar: usa o fade existente, cancela tentativas pendentes, zera moedas e estados da missão, restaura Time.timeScale e carrega Cave. HUD/timer ficam escondidos até concluir novamente o diálogo do NPC na Island; nesse momento o timer começa em 20:00.
- Menu Principal, pelo resultado ou Pause: limpa a partida e restaura Time.timeScale antes do carregamento. Retorna ao menu de opções existente sem repetir nickname/vídeo. GameManager e MoneyManager permanecem os mesmos singletons.

O timer interno fica em zero na fase pré-missão após reset, como antes; 1200 segundos são aplicados por StartMission. Não se inicia uma missão automaticamente ao recomeçar.

## Hierarchy nas duas cenas

```text
GameplayMenusCanvas
  MissionResultPanel (inativo)
    Card
      EndGameTitle
      EndGameMessage
      RestartButton
      MainMenuButton
  PausePanel (inativo)
    Card
      PauseTitle
      ResumeButton
      PauseSettingsButton
      PauseMainMenuButton
GameplayMenuController
  [PauseController + SettingsMenuUI]
  SettingsPanel (inativo; mesma UI editável das preferências)
MissionResultUI (controlador existente)
```

As interfaces estão salvas nas cenas, editáveis fora do Play Mode. GameplayMenusCanvas fica acima do gameplay; SettingsPanel fica acima do Pause e abaixo do fade de troca de cena. Não foi criado outro HUD, GameManager, MoneyManager ou sistema de mineração.

## Inspector e edição

Nenhuma atribuição manual obrigatória: os painéis, textos, botões, páginas, sliders e dropdowns foram conectados ao salvar as cenas com o Unity. Nenhum asset externo está faltando para esta tarefa.

Para editar a aparência, abra Cave ou Island e ative temporariamente o painel desejado fora do Play Mode. Edite Image, TextMeshPro e RectTransform nos filhos; ao terminar, deixe os painéis fechados. Preserve as referências do PauseController, MissionResultUI e SettingsMenuUI ao substituir objetos. Os textos de resultado são atualizados por MissionResultUI conforme o estado; estilos são editáveis no Inspector. As duas cenas têm UI local, portanto alterações visuais posteriores em uma não se propagam automaticamente à outra.

## Arquivos

Criados: Assets/Scripts/PauseController.cs e .meta; Tests/UIAuthoring/AuthorGameplayUI.cs e RunGameplay.ps1 (autoria em cópia isolada); este relatório e PauseEndGameValidationResults.txt.

Modificados:

- GameManager.cs: pausa, eventos, bloqueio e reset/saída da partida.
- PlayerClickMovement.cs: respeita bloqueio central de pausa/fim sem reescrever movimento.
- MiningSpot.cs: congela a coroutine enquanto pausado, mantendo a tentativa existente.
- MoneyManager.cs: recusa recompensa durante pausa.
- DialogueController.cs e ScrollInteraction.cs: bloqueiam ações de UI incompatíveis com pausa/fim.
- MissionResultUI.cs: título, mensagem, moedas e ações Recomeçar/Menu.
- SettingsMenuUI.cs: aceita o contexto do Pause além do Main Menu, sem duplicar preferências.
- MainMenuController.cs: reconhece retorno das cenas jogáveis ao menu principal.
- Cave.unity e Island.unity: painéis e referências locais.
- Tests/GameplayValidation/GameplayValidationRunner.cs: casos de pausa, mineração interrompida temporariamente, estados finais, reinício e saídas. A checagem antiga do background passou a aceitar objetos visuais editáveis sem exigir um nome fixo.

TimerUI, MoneyUI, MiningUI, recompensas, posições de Player/NPC/portas/obstáculos, Splash, vídeo e assets de áudio permanecem preservados.

## Testes

Execução reproduzível: `powershell -File Tests/GameplayValidation/Run.ps1`, em cópia isolada com preferências próprias. Os testes chamam o mesmo método usado por ESC e os eventos dos botões; o pressionamento físico da tecla e a apresentação visual ainda devem ser conferidos no Editor.

Conferir manualmente nas duas cenas: ESC durante exploração, diálogo e mineração; continuar; abrir/fechar dropdowns em Configurações; botão Voltar sem retomar o jogo; vitória/derrota; Recomeçar; saída ao menu; visibilidade e legibilidade em diferentes resoluções. Conferir também reprodução real do vídeo no menu, pois a suíte sem GPU simula seu callback de término.

Resultado: **105 verificações aprovadas (ALL PASSED)** e compilação sem erros no Unity. Relatório bruto em PauseEndGameValidationResults.txt. O erro conhecido de inicialização do índice UnityEditor.Search foi registrado separadamente, sem ignorar exceções dos scripts do jogo. Verificações estáticas confirmaram IDs/referências locais válidos e preservação de todos os blocos antigos fora da UI alterada, inclusive as malhas de obstáculos.

