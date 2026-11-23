# Etapa 11 — Introdução ilustrada na praia

## Experiência
Ao iniciar uma partida, a praia apresenta quatro pensamentos do personagem:
1. Eita... o que estou fazendo aqui?
2. O que aconteceu comigo? Eu não lembro de nada...
3. O que será que aconteceu? Espera... o que é aquela casa no horizonte?
4. Vou até ela para ver se alguém pode me ajudar.

O jogador avança pelo botão. A moldura usa pergaminho ilustrado com elementos da costa e o sprite original do personagem como retrato. O texto continua em TextMeshPro. Ao concluir, o movimento é liberado. A introdução não se repete ao retornar à praia e volta numa nova partida.

O cabeçalho de objetivo da praia fica oculto durante e após a introdução. Os objetivos continuam disponíveis ao mapa quando recebido. A barra inferior anteriormente removida permanece escondida. As HUDs dos demais cenários não foram alteradas.

## Arquivos
Criados:
- Assets/Art/UI/CoastalDialogue-v1.png e metadados: moldura gerada pela ferramenta integrada image_gen.
- Tests/UIAuthoring/AuthorBeachIntroduction.cs: integração incremental da arte, retrato e botão.
- Tests/GameplayValidation/BeachIntroductionValidation.cs: validação da introdução e do fluxo NPC.
- Documentation/BeachIntroduction/GenerationPrompt.txt: prompt final da imagem.

Modificados:
- Assets/Scripts/DialogueController.cs: modo de introdução do personagem reaproveitando abertura, avanço, bloqueio de movimento e fechamento existentes; pausa preserva a fala.
- Assets/Scripts/GameManager.cs: registro da introdução concluída e reset em nova partida.
- Assets/Scripts/StoryLocation.cs: opção de ocultar o cabeçalho da cena.
- Assets/Scenes/Beach.unity: UI configurada e cabeçalho oculto.
- Tests/GameplayValidation/StoryValidation.cs e MapAudioValidation.cs: avançam a introdução antes de testar a exploração.

## Configuração e assets
Nenhum arraste necessário no Inspector; nenhuma referência obrigatória faltante. A moldura fica no projeto. Não há dublagem nova; as falas são apresentadas em texto. O início da missão e a entrega do mapa continuam exclusivos da conversa com o NPC.

## Validação
- Compilação sem erros no Unity 6.
- 19 verificações aprovadas: BeachIntroduction/ValidationResults.txt.
- Validados início após fade, quatro falas, bloqueio de movimento, pausa/retomada, fim sem conceder missão/mapa, retorno à praia, nova partida e entrega posterior pelo NPC.
- Capturas da primeira/última fala revisadas em 1280×720; texto, retrato e botão legíveis sobre a arte.
- Cabeçalho da praia e barra inferior permanecem ocultos.
- Exceção conhecida do índice de busca UnityEditor.Search.SearchDatabase registrada separadamente; nenhuma exceção de gameplay aceita.
- Validação direcionada à introdução e continuidade do NPC; o percurso completo até os créditos não foi repetido nesta etapa.
