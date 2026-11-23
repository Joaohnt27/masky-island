# Indicação visual da mineração

O painel escuro da extração foi substituído por um cartão compacto de pergaminho, centralizado na parte inferior. Reutiliza DialoguePaper-v2.png e Pickaxe-v1.png, com fonte DynaPuff, texto verde escuro, barra dourada sobre fundo verde e botão CANCELAR. Permanece visível apenas durante a mineração, preservando o cenário e a animação do personagem.

## Arquivos
- Modificado: Assets/Scenes/Cave.unity — apresentação do WorldMiningPanel e de seus elementos.
- Criados: Tests/UIAuthoring/AuthorMiningIndicator.cs e Tests/GameplayValidation/MiningIndicatorValidation.cs — montagem no projeto de validação e checagem funcional/visual.
- Criados nesta pasta: relatório, captura e resultados da validação.

Nenhum script de gameplay foi alterado. Duração, recompensas, movimento e regras de mineração permanecem nos sistemas existentes. Não foram necessários novos assets de imagem nem configurações manuais no Inspector.

## Validação
Dez verificações em Play Mode na Unity 6000.6.0f1: painel oculto antes da ação, início da mineração, exibição da interface, progresso, bloqueio de movimento, acesso ao botão com ponteiro, cancelamento, restauração do movimento, coleta ao concluir e ocultação do painel. Captura revisada em 1280×720. A chance de coleta foi configurada em 100% somente durante a segunda tentativa do teste, sem salvar essa alteração na cena.

Backup da cena anterior em Builds/MiningIndicatorBackup/Cave.unity. Os resultados registram separadamente a exceção conhecida do índice de busca do Editor, sem ocultar erros de gameplay.
