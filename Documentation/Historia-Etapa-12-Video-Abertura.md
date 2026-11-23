# Etapa 12 — Vídeo de abertura e nome do jogador

## Fluxo
Jogar → Assets/Videos/intro_gameplay.mp4 → praia → pensamentos iniciais do personagem.

A transição que já aparece após confirmar o nickname continua separada. O mesmo VideoPlayer e painel existentes são reutilizados para o novo clipe ao clicar em Jogar. Os controles do menu ficam bloqueados até a entrada no gameplay, impedindo reproduções e cargas duplicadas. O vídeo é preparado com a tela coberta; o fim usa fade para chegar à praia. Clipe ausente, erro de reprodução ou prazo excedido permitem continuar o fluxo.

O título das falas do personagem usa PlayerPrefs/PlayerNickname, já salvo pela tela de nickname. O texto aparece literalmente, sem interpretar tags TMP. Se o nome estiver vazio, a identificação continua Você. O nome é uma identificação de interface para o jogador, e não uma fala em que o personagem afirma lembrar sua identidade: as falas de amnésia permanecem iguais. O NPC mantém seu próprio nome.

## Arquivos modificados
- Assets/Scripts/MainMenuController.cs: referência do clipe e ramificação do fluxo de vídeo existente para o gameplay; prazo de reprodução e prevenção de cliques repetidos.
- Assets/Scripts/DialogueController.cs: identificação das falas iniciais com o nickname salvo.
- Assets/Scenes/MainMenu.unity: referência a intro_gameplay.mp4.
- Tests/GameplayValidation/StoryValidation.cs e BeachIntroductionValidation.cs: aguardam a conclusão do vídeo antes de verificar a praia.

## Arquivos criados
- Tests/GameplayValidation/GameplayVideoValidation.cs: reprodução real, sequência menu/praia, nome, fallback e bloqueio de repetição.
- Documentation/GameplayVideo: resultados e capturas da validação.

## Configuração e assets
Nenhuma configuração manual no Inspector. Usa o vídeo já fornecido no projeto, sem gerar outro vídeo, cena ou sistema de nome. Mantém a integração existente de volume do VideoPlayer. Não altera o encerramento nem os créditos.

## Testes
- Compilação Unity 6 sem erros.
- 13 verificações aprovadas: GameplayVideo/ValidationResults.txt.
- Reprodução real do MP4, avanço de frames, transição automática para praia, bloqueio de cliques repetidos e preservação da missão/mapa.
- Nome de teste com acento exibido na introdução; identificação literal sem rich text; nome vazio retorna a Você. Preferências do teste restauradas ao terminar.
- Clipe ausente testado: jogo segue para a praia. Erro de codec e limite de tempo têm tratamento no código, mas não foram induzidos nesta execução.
- Capturas do vídeo e da fala com nome revisadas em 1280×720: GameplayVideo/gameplay-intro-video.png e named-beach-dialogue.png.
- Unity informou ajuste automático dos timestamps H.264 do arquivo; a reprodução e a conclusão passaram. A exceção conhecida do índice UnityEditor.Search.SearchDatabase foi registrada separadamente, sem aceitar exceções de gameplay.
- Validação focada na abertura; o percurso completo até os créditos não foi repetido nesta etapa.
