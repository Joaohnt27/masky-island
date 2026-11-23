from pathlib import Path
from docx import Document
from docx.shared import Cm, Pt, RGBColor
from docx.oxml import OxmlElement
from docx.oxml.ns import qn

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).parent / 'Mask Island Historia e Motivacao.docx'
doc = Document()
sec = doc.sections[0]
sec.page_width, sec.page_height = Cm(21), Cm(29.7)
sec.top_margin = sec.bottom_margin = Cm(2)
sec.left_margin = sec.right_margin = Cm(2.3)
normal = doc.styles['Normal']
normal.font.name, normal.font.size = 'Calibri', Pt(11)
normal.paragraph_format.space_after = Pt(8)
normal.paragraph_format.line_spacing = 1.12
for name, size in [('Title', 30), ('Subtitle', 16), ('Heading 1', 20), ('Heading 2', 13)]:
    style = doc.styles[name]
    style.font.name = 'Calibri'
    style.font.size = Pt(size)
    style.font.color.rgb = RGBColor(0, 0, 0)
    style.paragraph_format.space_after = Pt(10)
    style.paragraph_format.keep_with_next = True
normal.element.get_or_add_rPr().append(OxmlElement('w:lang'))
normal.element.rPr[-1].set(qn('w:val'), 'pt-BR')
doc.core_properties.title = 'Mask Island Historia e Motivacao'
doc.core_properties.subject = 'Narrativa completa e motivação do jogo'
doc.core_properties.author = 'Mask Island'

def p(text):
    doc.add_paragraph(text)

def h(text):
    doc.add_heading(text, level=2)

def page(title):
    doc.add_page_break()
    doc.add_heading(title, level=1)

doc.add_paragraph('Mask Island', 'Title')
doc.add_paragraph('História completa e motivação do jogo', 'Subtitle')
p('Uma aventura sobre recuperar a memória, encontrar um propósito e devolver a luz a uma ilha cercada pelo mar.')
pic = doc.add_picture(str(ROOT / 'Assets/Art/Backgrounds/Story/01-praia.png'), width=Cm(16.4))
pic._inline.docPr.set('descr', 'Praia da ilha com um barco na areia, uma casa no caminho e o farol ao fundo.')
h('A proposta')
p('Mask Island acompanha um viajante que desperta em uma praia após uma tempestade, sem compreender como chegou ali ou qual era sua ligação com aquele lugar. Uma casa no horizonte parece ser a primeira chance de conseguir ajuda. O que começa como uma busca por respostas se transforma na descoberta de uma responsabilidade esquecida: ele era o guardião do farol e voltou para restaurar sua luz.')
p('A aventura se desenvolve em cinco cenários: praia, casa, centro da ilha, caverna e farol. A exploração, as conversas e os enigmas recuperam três lembranças que dão sentido à jornada. Cada objeto encontrado tem uma função concreta, e cada tarefa conduz à próxima descoberta.')
h('O objetivo da jornada')
p('O jogador precisa reconstruir a história do protagonista e reacender o farol. A vitória acontece quando ele conclui o desafio final do mecanismo, devolvendo à ilha a referência que permite encontrar o caminho pelo mar. O encerramento em vídeo apresenta a conclusão da aventura antes dos créditos.')

page('O despertar e a procura por ajuda')
h('A tempestade')
p('A história começa no mar. Uma tempestade atinge o barco do protagonista e interrompe sua viagem. Quando a sequência de abertura termina, ele está na praia de uma ilha desconhecida. Sobreviveu, mas suas lembranças não o acompanharam: não sabe explicar o acidente, reconhecer o lugar ou entender o motivo de estar ali.')
p('Seu nome é o escolhido pelo jogador. Esse nome identifica suas falas sem revelar antecipadamente seu passado. O mistério principal está em quem ele era, no que viveu e na razão de ter voltado à ilha.')
h('Um diário entre a areia e o mar')
p('Ainda confuso, ele observa o ambiente e tenta organizar os pensamentos: “Eita... o que estou fazendo aqui? O que aconteceu comigo? Eu não lembro de nada...” Ao olhar para o caminho, percebe uma casa e decide procurar alguém que possa ajudá-lo.')
p('Antes de partir, encontra um livro na areia. “Olha, tem um livro ali. Vou pegar para ver o que tem.” Ao examiná-lo, descobre que é um diário com todas as páginas vazias. Não há respostas escondidas nele, mas existe uma oportunidade: registrar o que descobrir, para não perder novamente o rumo.')
p('Ele guarda o diário e escreve sua primeira tarefa: procurar ajuda na casa. Esse gesto inicia uma mudança pequena, mas importante. Mesmo sem conhecer o passado, já consegue decidir o próximo passo. O caderno passa a acompanhar a viagem, recebendo objetivos e riscos de caneta quando as tarefas são concluídas.')
h('A casa e o aviso sobre a mesa')
p('A casa parece acolhedora, com móveis simples, uma cama e uma mesa. Entretanto, ninguém está ali para responder às suas perguntas. Sobre a mesa, um aviso oferece uma direção: “Precisa de ajuda? Estou trabalhando no poço, no centro da ilha.”')
p('A descoberta transforma a esperança vaga de encontrar alguém em uma missão clara. Ele registra no diário que deve procurar o trabalhador no centro. Ao lado da cama há uma mochila, mas ela pertence a outra pessoa; o protagonista ainda não tem autorização para levá-la.')
p('A casa deixa de ser apenas um abrigo. Ela se torna um ponto de retorno da história: primeiro oferece uma pista sobre onde encontrar ajuda e, mais tarde, ajuda o personagem a compreender suas próprias lembranças.')

page('O estranho e a primeira lembrança')
h('O encontro no centro da ilha')
p('Perto do poço, o protagonista encontra o trabalhador indicado pelo aviso. Para ele, aquele homem ainda é apenas o Estranho. O homem ouve sobre o despertar na praia e responde de maneira enigmática: “Você acordou na praia? Então a ilha trouxe mais alguém de volta.”')
p('O Estranho explica que, naquele lugar, as lembranças podem se perder, mas deixam pistas. Também apresenta o problema que afeta a ilha: o farol está apagado. Sem sua luz, os barcos não encontram uma orientação segura para sair. Recuperar três lembranças é o caminho para compreender como devolver a luz ao farol.')
p('Ele não entrega todas as respostas. Orienta o viajante a voltar à casa e investigar a inscrição sobre a mesa. A pista fala sobre as mudanças do céu e deve ser interpretada por meio dos símbolos disponíveis.')
h('Preparação para a busca')
p('Antes da partida, o Estranho oferece recursos para a jornada. Entrega um mapa da ilha, que permite consultar os lugares e viajar rapidamente entre aqueles que já foram visitados. Também autoriza o protagonista a buscar sua mochila, deixada ao lado da cama na casa, para guardar os objetos que encontrar.')
p('O homem indica ainda uma ferramenta que será necessária depois: uma picareta na entrada da caverna, à esquerda, encostada nas pedras. A orientação fica registrada no diário. Assim, o jogador entende onde buscar o equipamento e por que ele será útil antes de tentar minerar.')
h('A luz já fazia parte de sua vida')
p('De volta à casa, o protagonista recolhe a mochila com a permissão recebida. Agora pode transportar os cristais que encontrará mais adiante. Em seguida, examina o bilhete e procura compreender a sequência descrita pela pista.')
p('O enigma do céu é resolvido ao selecionar sol, lua e estrela, nessa ordem. A inscrição reage e desperta a primeira lembrança: ele cuidava da luz do farol. O edifício distante já fez parte de sua vida; sua ligação com a ilha é anterior ao despertar na praia.')
p('A descoberta altera sua motivação. Ele ainda quer entender o que aconteceu e encontrar uma saída, mas o farol começa a representar algo pessoal. Existe uma tarefa ligada ao seu passado que permanece incompleta. Ao conversar novamente com o Estranho, recebe a orientação de procurar os cristais na caverna.')

page('Os cristais e o caminho de volta')
h('A exploração da caverna')
p('Na entrada da caverna, o protagonista encontra a picareta exatamente no lugar indicado. Com a mochila e a ferramenta, pode se aproximar dos três veios de rocha e começar a minerar. Os cristais não aparecem de imediato: cada tentativa exige trabalho, e algumas não encontram nada. Ele precisa continuar procurando.')
p('A mineração torna a descoberta uma ação do próprio personagem. Em vez de receber os fragmentos prontos, ele explora as rochas, acompanha a extração e guarda os achados no inventário. Os três cristais têm formas distintas: um círculo dourado, um triângulo turquesa e um quadrado roxo.')
h('O altar e a segunda lembrança')
p('Ao reunir os três cristais, o protagonista pode resolver o enigma do altar. Na pedra existem moldes correspondentes às formas encontradas. Ele seleciona cada cristal e o coloca no encaixe adequado: círculo à esquerda, triângulo ao centro e quadrado à direita.')
p('Quando o altar fica completo, a segunda lembrança se revela: na noite da tempestade, o farol se apagou. A escuridão da ilha e a tempestade passam a fazer parte da mesma história. Os cristais permanecem encaixados, deixando uma marca visível do que ele conseguiu realizar.')
p('Essa revelação não significa que o protagonista tenha provocado a falha. Ela mostra quando tudo mudou e aproxima o jogador da compreensão de sua ligação com o farol. Ainda falta descobrir por que ele estava voltando à ilha.')
h('A última visita à casa')
p('A próxima pista leva novamente à casa. O Estranho orienta o viajante a reler o bilhete, agora com as duas lembranças recuperadas. O lugar conhecido ganha outro sentido: aquilo que antes parecia apenas um conjunto de símbolos começa a contar a história de um retorno.')
p('No novo enigma, a sequência barco, peixe e farol representa o caminho da volta. Ao resolvê-lo, o protagonista recupera a terceira lembrança: ele retornou para terminar o que havia começado e restaurar a luz do farol.')
p('As três descobertas finalmente se conectam. Ele era o guardião, a luz se apagou na noite da tempestade e seu retorno tinha um propósito. Já não segue as pistas apenas porque alguém lhe pediu. Entende a responsabilidade que o trouxe até ali e decide concluir sua tarefa.')

page('O farol e o encerramento')
h('Uma decisão consciente')
p('Com as três lembranças recuperadas, o protagonista segue do centro da ilha até o farol. A construção que antes parecia distante e estranha agora pertence à sua história. O Estranho cumpriu seu papel de orientar; o último passo depende do viajante.')
p('Diante do mecanismo, ele precisa transformar o que descobriu em ação. O desafio final exige pressionar as teclas indicadas no momento certo, em uma sequência de comandos. O ritmo traz tensão ao instante de reacender a luz, sem transformar toda a exploração em uma corrida contra o tempo.')
p('Se errar, o personagem pode tentar novamente. As lembranças e as tarefas concluídas permanecem. A dificuldade representa a tentativa de colocar o mecanismo em funcionamento, não a perda de tudo o que aprendeu.')
h('A luz atravessa a névoa')
p('Ao concluir o desafio, o farol volta a iluminar o mar. A luz atravessa a névoa e oferece novamente uma direção a quem precisa navegar. O protagonista termina a tarefa que motivou seu retorno e recupera o sentido de sua própria jornada.')
p('O vídeo de encerramento entra após o sucesso no mecanismo. O desfecho reúne as revelações centrais: ele era o guardião do farol e voltou para terminar o que começou. Com o caminho iluminado, a ilha deixa de parecer uma prisão. Existe, finalmente, a possibilidade de voltar para casa. Após a cena final, o jogo segue para os créditos.')
h('O que a história mantém em aberto')
p('A natureza da ilha e o quanto o Estranho sabe podem continuar misteriosos. A aventura não precisa explicar uma maldição, revelar uma identidade secreta para o NPC ou atribuir culpa ao protagonista. O arco principal se encerra quando ele recupera seu propósito e restaura a luz.')
h('A transformação do protagonista')
p('No início, sua pergunta é “O que aconteceu comigo?”. Ao longo da aventura, ela se torna “Qual era a minha ligação com este lugar?”. No final, ele já pode agir sobre uma resposta: era o guardião e voltou para restaurar o farol. A conclusão acontece por uma escolha sustentada pelas lembranças recuperadas.')

page('A motivação por trás do jogo')
h('Por que o personagem continua')
p('A motivação inicial é imediata e humana: procurar ajuda depois de acordar sozinho e sem memória. O aviso da casa oferece uma pessoa para encontrar. A conversa no centro oferece um objetivo maior. A primeira lembrança cria um vínculo pessoal com o farol; a segunda liga esse vínculo à tempestade; a terceira explica o retorno. Essa progressão dá uma razão compreensível para cada deslocamento.')
p('No final, recuperar a memória e ajudar a ilha se tornam partes da mesma decisão. Acender o farol permite concluir uma responsabilidade esquecida e abrir o caminho de volta. O protagonista encontra um propósito ao participar ativamente da solução.')
h('Por que o jogador continua')
p('A curiosidade sustenta a exploração: quem é esse viajante, por que estava no mar e o que o liga ao farol? As respostas chegam aos poucos, acompanhadas de mudanças concretas. O diário ganha registros, a mochila recebe itens, o mapa revela destinos conhecidos e o altar conserva os cristais colocados. O progresso pode ser percebido no mundo e nos objetos do personagem.')
p('As tarefas são simples de compreender, mas variam entre observar, conversar, interpretar pistas, procurar recursos e executar o desafio final. O diário mantém a próxima ação clara, enquanto os retornos à casa permitem descobrir novos sentidos em um cenário familiar. A progressão narrativa é linear, mesmo com liberdade para circular pela ilha.')
h('O papel dos objetos na história')
p('O diário funciona como uma memória construída no presente: começa vazio e passa a registrar o que o personagem escolhe fazer. A mochila permite levar consigo as descobertas. O mapa oferece orientação e torna a ilha menos desconhecida. A picareta dá ao protagonista os meios para procurar aquilo de que precisa. Os cristais unem exploração e resolução de enigmas, enquanto o farol representa uma direção compartilhada por quem está em terra e no mar.')
h('A experiência que o jogo quer oferecer')
p('O visual ilustrado, os cenários costeiros e o personagem mascarado sustentam uma aventura de mistério acessível, com momentos de acolhimento e descoberta. A ausência de uma contagem regressiva geral permite observar o ambiente e compreender as pistas. A tensão fica concentrada no desafio final, quando a história já deu ao jogador um motivo para se importar com o resultado.')
p('Como projeto acadêmico, o jogo reúne exploração, interação, diálogo, inventário, coleta, enigmas e conclusão em uma aventura curta e coerente. Sua mensagem central é que encontrar uma direção pode começar com uma ação pequena: anotar uma tarefa, seguir uma pista, pedir ajuda. Ao reconstruir essas etapas, o personagem encontra condições de iluminar novamente o próprio caminho e o de outras pessoas.')

OUT.parent.mkdir(parents=True, exist_ok=True)
doc.save(OUT)
print(OUT)
print('Parágrafos:', len(doc.paragraphs))
print('Palavras:', sum(len(p.text.split()) for p in doc.paragraphs))
