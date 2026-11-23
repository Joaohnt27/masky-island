# Mineração na Island, Splash e configurações

Pause e painéis de fim de jogo atuais: [PauseAndEndGame.md](PauseAndEndGame.md).

Detalhes da revisão mais recente de fades e trocas rápidas de cena: [Transitions.md](Transitions.md).

Guia atual para editar créditos e a aparência das configurações: [CreditsAndEditableSettings.md](CreditsAndEditableSettings.md).

## Fluxo atual

Splash → clique esquerdo → MainMenu (nickname) → vídeo existente → menu principal → Jogar → Cave → porta para Island → NPC → diálogo → missão → área de mineração na Island.

O NPC permanece na Island, como no projeto encontrado. Não foram movidos NPC, Player, portas ou obstáculos existentes. O vídeo toca após confirmar o nickname e antes de exibir o menu principal. Jogar entra diretamente na Cave.

## Mining

- Removidos de Cave: MiningSpot, MiningUI, MiningAreaHint e a árvore MinigPanel adicionada para mineração (título, botão, resultado, Slider e seus filhos).
- Reaproveitado o MinigPanel que já existia em Island, com a apresentação e configurações do painel funcional. Criada uma única instância dos mesmos componentes MiningSpot e MiningUI em Island, com referências ao Player e à UI locais.
- A detecção continua sendo por distância 3D: raio de 2 unidades. Não usa Collider, Rigidbody ou tecla E. Entrar disponibiliza o painel; sair oculta. O botão inicia a coroutine existente de 3 segundos.
- Scripts MiningSpot.cs e MiningUI.cs preservados byte a byte, incluindo sorteio, cancelamento e envio das moedas ao MoneyManager.
- GameManager, MoneyManager, timer e HUD existentes preservados. Nenhuma nova instância desses sistemas foi adicionada nesta etapa.
- As duas falas que orientavam retornar à Cave foram atualizadas para a área à esquerda da Island. A lógica do diálogo não mudou.
- **Posição provisória:** MiningSpot em `(-3, -4, 0)`, dentro da área caminhável, com alcance de 2. O enquadramento artístico definitivo deve ser conferido/ajustado no Unity. O gizmo amarelo mostra o raio. Nenhum objeto do cenário foi deslocado para acomodar essa área.

Referências já atribuídas: MiningSpot.Player → PlayerClickMovement da Island; MiningUI → MiningSpot, MinigPanel, MinigButton, MinigProgressBar e MinigResultText da Island. On Click do MinigButton continua vazio no Inspector: MiningUI registra o listener em código.

## Splash

- Nova cena: Assets/Scenes/Splash.unity, incluída como primeira cena habilitada no build.
- Novo script: SplashController.cs. Detecta `Mouse.current.leftButton.wasPressedThisFrame` pelo New Input System e aguarda a animação do clique e carrega MainMenu uma única vez.
- Usa o background Ilha.png já existente, com uma camada escura e texto "Clique para começar". Não contém nickname, missão ou botão Jogar.
- SplashParticles.cs desenha 48 partículas douradas no Canvas, com subida, oscilação e variação de brilho. O clique cria uma explosão radial e um anel no ponto do mouse, seguidos de fade de 0,85 segundo. Usa tempo real, sem bloquear a thread ou alterar o sorteio de mineração.
- O objeto Splash Particles é criado apenas na cena atual durante Play Mode. Não exige texturas, materiais, prefab ou ParticleSystem externo. SplashController permite ajustar Particle Count, Particle Color e Click Duration no Inspector; Splash Canvas já está atribuído.
- Layout, background e posição do texto editados pelo usuário foram preservados.
- Build: Splash, MainMenu, Cave, Island. O perfil Windows já utiliza a lista global; não foi criado outro perfil.

## Main Menu

- MainMenuController.cs: confirmar nickname valida e salva PlayerNickname, inicia o vídeo e oculta os painéis de entrada/menu; o término do vídeo abre o menu principal. Jogar carrega Cave. Repetição de cliques é bloqueada durante a transição.
- VideoPlayer, clip, RenderTexture, RawImage e painel de transição preservados. A configuração serializada do VideoPlayer é idêntica à anterior.
- Uma falha de reprodução gera aviso e permite continuar para o menu principal. A reprodução real e o codec devem ser conferidos na máquina de apresentação.
- SettingsButton agora chama SettingsMenuUI.Open. Jogar, Créditos e Sair continuam nos objetos existentes. Créditos agora abre Credits.unity, com rolagem editável e retorno ao menu sem repetir a introdução.

## Configurações

Novos scripts:

- SettingsManager.cs: uma instância persistente criada antes de carregar a primeira cena, inclusive quando o teste começa diretamente em MainMenu/Cave/Island. Carrega preferências e aplica áudio/vídeo; não guarda referências às interfaces.
- SettingsAudioChannel.cs: adaptador de categoria para AudioSource e para a saída Direct do VideoPlayer existente. Selecionar Music ou SoundEffects no Inspector para futuras fontes.
- SettingsMenuUI.cs: controla a árvore SettingsPanel salva na MainMenu, usando UGUI e TextMeshPro. Oferece abas Áudio, Vídeo e Controles, e botão Voltar. O controlador permanece fora do painel ocultável.

O painel agora está salvo na cena e pode ser editado fora do Play Mode em MainMenuController → SettingsPanel. Sliders, dropdowns, páginas, textos e botões já estão atribuídos no SettingsMenuUI; os callbacks internos são conectados uma vez em Awake. Consulte o guia de edição acima.

### Áudio

- Volume Geral controla AudioListener.volume.
- Música e Efeitos controlam volumes por categoria. SettingsAudioChannel preserva o volume base da fonte e multiplica pela preferência da categoria, evitando multiplicações cumulativas.
- O VideoPlayer existente usa saída Direct, que recebe Geral × Música pelo adaptador já atribuído ao TransitionVideoPlayer.
- Não havia AudioMixer nem arquivos separados de música/efeitos. Não foram adicionados pacotes, um mixer desnecessário ou sons inventados. Os três sliders são funcionais; testar Música/Efeitos com novos AudioSources exige inserir os clips correspondentes e adicionar o adaptador de categoria.

### Vídeo

- Resoluções obtidas de Screen.resolutions, sem duplicatas de largura/altura. Inclui o tamanho real da janela atual, que também serve de alternativa quando a plataforma/Editor não enumera modos.
- Fullscreen usa FullScreenWindow; Janela usa Windowed. Não altera o estado de fullscreen do próprio Editor.
- Qualidade lista QualitySettings.names (Mobile e PC na configuração atual) e aplica QualitySettings.SetQualityLevel. Nenhum nível ou Render Pipeline foi criado/alterado.
- Selecionar as opções e clicar em APLICAR. As escolhas são salvas e aplicadas ao inicializar o jogo novamente. Resoluções salvas que não existem no monitor atual usam uma opção válida do ambiente atual.

### Controles

- Mouse esquerdo: clicar no chão para mover e em botões para selecionar.
- E: interagir com NPCs e pergaminhos próximos.
- ESC: fechar uma lista aberta ou voltar das Configurações ao menu. Em Cave e Island, ESC agora também controla o Pause, conforme o relatório de pausa e fim de jogo.
- Sem remapeamento de teclas nesta etapa.

### Persistência

PlayerPrefs: MasterVolume, MusicVolume, SFXVolume, Fullscreen, ResolutionWidth, ResolutionHeight e QualityLevel. PlayerNickname continua sendo salvo pelo menu existente.

Os sliders aplicam áudio imediatamente e atualizam as preferências em memória. Save é executado ao voltar/fechar as configurações, pausar a aplicação ou encerrar. Vídeo é salvo ao aplicar. Nada é salvo a cada frame.

## Arquivos modificados nesta etapa

- Assets/Scenes/Cave.unity e Island.unity: somente transferência/configuração dos objetos de mineração.
- Assets/Scenes/MainMenu.unity: SettingsMenuUI, callback de Configurações e SettingsAudioChannel no vídeo.
- Assets/Scripts/MainMenuController.cs: nickname → vídeo → menu; Jogar → Cave.
- Assets/Scripts/DialogueController.cs: duas falas de orientação sobre a localização da mineração.
- ProjectSettings/EditorBuildSettings.asset: Splash na primeira posição.
- Tests/GameplayValidation: entrada pela Splash, testes de menu/preferências, mineração em Island e preferências isoladas do jogo real.
- Documentation/Gameplay.md: documentação da localização/fluxo atualizados.

Criados: Splash.unity, SplashController.cs, SplashParticles.cs, SettingsManager.cs, SettingsAudioChannel.cs, SettingsMenuUI.cs, respectivos arquivos .meta, este relatório e MenuValidationResults.txt.

## Ajuste posterior: introdução e efeitos da Splash

- Criados: `Assets/Scripts/SplashParticles.cs` e seu `.meta`.
- Modificados: `MainMenuController.cs` (introdução após nickname), `SplashController.cs` (partículas e coroutine de clique), `Splash.unity` (somente quatro campos do controlador).
- Atualizados: `Tests/GameplayValidation/GameplayValidationRunner.cs`, `Documentation/Gameplay.md`, este relatório e `MenuValidationResults.txt`.
- Não foram alterados MainMenu.unity, Cave.unity, Island.unity, prefabs, mineração, movimento, gerenciadores, vídeo, RenderTexture ou controles das configurações nesta revisão.
- O efeito utiliza geometria leve de UI, com brilho por transparência. Não requer novos assets, animações importadas ou pacotes. Quantidade e cor das partículas e duração do clique são ajustáveis no SplashController.
- Não há nova configuração obrigatória no Inspector. Conferir visualmente as partículas e o clique em Play Mode, incluindo cliques nos cantos e diferentes proporções de tela. Conferir que o vídeo toca após confirmar nickname e que o menu aparece somente depois dele.

## Ações no Unity Editor

1. Conferir/ajustar a posição artística final do MiningSpot na Island, usando o gizmo de raio 2. A posição provisória foi escolhida dentro da área caminhável sem mover o cenário.
2. Testar a apresentação visual da Splash e das configurações nas resoluções usadas pelo grupo; em Play Mode, o SettingsPanel aparece sob MainMenuController.
3. Conferir reprodução/áudio do vídeo após confirmar o nickname, e testar resolução/fullscreen em um executável Windows.
4. Quando houver músicas e efeitos separados: adicionar AudioSource com clip ao objeto e SettingsAudioChannel, escolhendo a categoria. A referência AudioSource pode ficar vazia quando o componente está no mesmo objeto. Fontes roteadas pelo mixer no futuro precisarão de integração deliberada; não existe mixer hoje.

Nenhuma ação manual para adicionar Splash ao build, atribuir sliders, conectar botões ou corrigir referências das cenas é necessária: isso já está configurado. A animação de mineração continua dependendo dos sprites/clips/controller ausentes, conforme o relatório anterior.

## Testes

Verificação estática: quatro cenas com IDs únicos, referências locais válidas, relações pai/filho e proprietários de componentes consistentes; exatamente um MiningSpot em Island, zero em Cave; gerenciadores e HUDs sem novas duplicatas. Hashes confirmam preservação dos scripts de mineração/missão/movimento; comparação do VideoPlayer e nickname confirma preservação das configurações existentes.

Compilação direta com Roslyn e as referências do Unity: todos os 56 arquivos C# de runtime, incluindo SplashParticles, sem erros. Os avisos CS0649 são os campos serializados preenchidos pelo Inspector; há também um aviso de campo não usado em um exemplo do TextMeshPro e de API de busca antiga no fallback do Canvas, ainda suportada pelo Unity instalado.

Suíte reproduzível: `powershell -File Tests/GameplayValidation/Run.ps1`. Usa a cópia Temp/GameplayWork/ValidationProject, com Product Name próprio para não alterar os PlayerPrefs do jogador. Não entrar em duas execuções simultâneas dessa suíte. O resultado fica em validation-results.txt dentro da cópia.

**Resultado: 62 verificações aprovadas, ALL PASSED.** Inclui bootstrap/avanço da Splash, presença das partículas, espera pela animação do clique, nickname → vídeo → menu, bloqueio de avanço durante a introdução, referência ao vídeo/RenderTexture, botão Configurações, volumes Geral/Música/Efeitos, áudio direto do vídeo, AudioSources das duas categorias, Voltar sem recarregar a cena, aplicação/persistência de vídeo, recarga das preferências, término da transição, ausência de mineração em Cave, MiningSpot único na Island e todos os cenários de mineração/missão da suíte anterior. Relatório preservado em [MenuValidationResults.txt](MenuValidationResults.txt).

A exceção de inicialização do índice `UnityEditor.Search.SearchDatabase`, já observada anteriormente, ocorreu novamente e foi registrada como nota de infraestrutura. Nenhuma exceção dos scripts do jogo foi ignorada. A compilação da suíte também emite avisos de APIs de busca antigas; eles não impedem sua execução no Unity 6.

A checagem visual, clique físico do mouse e reprodução real do vídeo não são cobertos pelo teste sem GPU. A suíte aciona o método de avanço da Splash e o callback de conclusão do vídeo, valida os botões e exercita a mineração em Play Mode.

## Cancelar nickname

O botão CancelButton da tela de nickname chama MainMenuController.CancelarNickname e retorna à Splash com o SceneFade existente. Cliques repetidos são bloqueados; o texto digitado não é salvo pelo cancelamento. O nickname previamente salvo permanece inalterado. Ao clicar novamente na Splash, a tela de nickname volta a abrir normalmente.

Alterados nesta revisão: Assets/Scripts/MainMenuController.cs, somente o callback de CancelButton em Assets/Scenes/MainMenu.unity, Tests/GameplayValidation/GameplayValidationRunner.cs e esta documentação. Nenhum novo script, asset ou configuração manual no Inspector é necessário. O teste automatizado aciona o evento do botão; a aparência do fade e o clique físico devem ser conferidos no Editor.

Validação do cancelamento no Unity: aprovados o evento do botão, bloqueio de repetição, destino Splash, preservação do nickname salvo e reabertura da entrada. Compilação sem erros. A suíte geral não concluiu: uma checagem antiga procura o objeto SettingsBackground, ausente com esse nome no estado atual da UI. Nenhum objeto das configurações foi alterado para satisfazer esse teste. A checagem de créditos foi ajustada para aceitar o texto personalizado, sem exigir o placeholder original. Resultado bruto: CancelNicknameValidationResults.txt. Não se declara aprovação completa da suíte nesta revisão.
