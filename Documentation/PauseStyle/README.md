# Menu de pausa — visual costeiro

O menu usa o pergaminho ilustrado já presente nos diálogos, com cordas, conchas e lanterna. O cenário permanece visível sob uma camada escura. O título utiliza Kablammo e os controles usam DynaPuff, fontes já incluídas no projeto.

## Alterações
- Beach, House, Island, Cave e Lighthouse: painel horizontal com título à esquerda e três ações à direita; Continuar em verde-petróleo, opções secundárias em tons de papel e dourado, realce ao passar o cursor e dica de ESC.
- Configurações acessadas pela pausa: cores e fontes consistentes, sliders com preenchimento verde-petróleo e controles atualizados para WASD, mouse, I, M, J e ESC.
- Os botões e suas referências existentes foram preservados. Nenhuma mudança no estado de pausa, movimento, mineração ou no menu principal.

## Arquivos de autoria e verificação
- Tests/UIAuthoring/AuthorPauseStyle.cs: aplicação da aparência às cinco cenas na cópia isolada.
- Tests/GameplayValidation/PauseStyleValidation.cs: pausa, acesso aos botões por raycast, configurações e retomada nas cinco cenas; capturas do resultado.
- Nenhum novo asset gráfico ou script de runtime necessário. Nenhuma configuração manual pendente no Inspector.

## Verificação
Resultados e capturas acompanham este relatório após a validação em Play Mode.

As ilustrações existentes dos botões das configurações foram mantidas e associadas ao realce de seleção. A página de controles foi reorganizada em um guia único, removendo as instruções antigas da tecla E.


Validação final: 55 verificações aprovadas em Play Mode no Unity 6000.6.0f1. Capturas de pausa, áudio, vídeo e controles revisadas. Cenas copiadas com hashes conferidos. Backup em Builds/PauseStyleBackup. A execução registrou a exceção preexistente do índice de busca do Editor, sem erros de gameplay.

