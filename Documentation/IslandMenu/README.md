# Menu da ilha

## Visual e navegação
- Nova arte IslandBeforeStorm-v1.png aplicada ao menu principal, à escolha do nome, às configurações e aos créditos.
- Menu principal com título MASKY ISLAND, botões de pergaminho à esquerda e realce ao selecionar ou passar o ponteiro. O nome acompanha o título já cadastrado nos créditos.
- Configurações em pergaminho, fonte DynaPuff, cores verde e dourado, abas de áudio/vídeo/controles e campos legíveis.
- Tela de nome harmonizada, mantendo a entrada de texto e as ações Continuar/Voltar.
- Créditos em pergaminho sobre a ilha, com rolagem e retorno ao menu. Nomes, contribuições e textos existentes foram preservados, incluindo campos de autoria ainda pendentes.
- Vídeos pre_menu e intro_gameplay e as funções existentes foram mantidos. O fundo novo é uma imagem estática.

## Arquivos
Modificados: Assets/Scenes/MainMenu.unity e Assets/Scenes/Credits.unity.

Configurado o importador de Assets/Art/Backgrounds/Menu/IslandBeforeStorm-v1.png por seu arquivo meta. A imagem já havia sido criada na etapa anterior. Reutilizados DialoguePaper-v2.png e a fonte DynaPuff do projeto.

Criados: Tests/UIAuthoring/AuthorIslandMenu.cs, Tests/GameplayValidation/IslandMenuValidation.cs, relatório, resultados e seis capturas nesta pasta. Nenhum script de gameplay foi alterado.

## Configuração e assets
Nenhuma configuração manual no Inspector é necessária. As cenas mantêm as referências dos botões e dos controladores existentes. Nenhum asset visual adicional está pendente. Os créditos ainda contêm os campos de áudio e agradecimentos que já estavam por preencher; não foram inventados autores.

## Validação
Unity 6000.6.0f1 em Builds/PV: 22 verificações de navegação e interação. Conferidos acesso aos botões, abertura/fechamento das configurações, abas, slider de áudio, dropdown de vídeo, Esc fechando a lista, créditos com rolagem e retorno, controles de nome, início do vídeo de gameplay e entrada na praia após pular. O volume alterado pelo teste foi restaurado antes de fechar as configurações.

Capturas revisadas em 1280×720. O teste não encerra o aplicativo pelo botão Sair nem altera a resolução física da sessão; os eventos existentes foram preservados. A exceção conhecida do índice de busca da Unity é registrada separadamente dos erros de gameplay.

Backup das cenas anteriores em Builds/IslandMenuBackup.
