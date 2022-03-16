const nameInput = $('#task_Name')
const descriptionInput = $('#task_Description')
const listOfExecutorsInput = $('#task_ListOfExecutors')
const statusSelect = $('#task_Status')
const dateOfRegistrationInput = $('#task_DateOfRegistration')
const dateOfCompleteInput = $('#task_DateOfComplete')
const plannedComplexityInput = $('#task_PlannedComplexity')
const actualExecutionTimeInput = $('#task_ActualExecutionTime')
const inputsArray = [nameInput, descriptionInput, listOfExecutorsInput,
    dateOfRegistrationInput, dateOfCompleteInput,
    plannedComplexityInput, actualExecutionTimeInput]

const taskForm = $('#task-form')

let taskTreeFocusedElement: JQuery<HTMLDivElement>
let currentErrorAlert: JQuery<HTMLDivElement>

function showErrorAlert(errorName: string) {
    if (currentErrorAlert !== undefined) {
        currentErrorAlert.remove()
    }

    $.get("../html/alert.html", function (html) {
        $.ajax({
            url: `api/ErrorMessage/${errorName}`,
            cache: false,
            success: function (msg) {
                html = $(html).prepend(msg).get(0)
                taskForm.after(html)
                currentErrorAlert = $('.alert')
                setTimeout(function () {
                    currentErrorAlert.remove()
                }, 4000)
            }
        })
    })
}

function traverseChildren(node, callback: Function) {
    const queue = []
    queue.push(node)

    while (queue.length !== 0) {
        node = queue.shift()

        callback(node)
        $.each(node.childrenTasks, function (_, val) {
            queue.push(val)
        })
    }
}

$('.task-node-name').on('click', function (event) {
    event.stopPropagation()

    if (taskTreeFocusedElement !== undefined) {
        taskTreeFocusedElement.removeClass('task-node-focused')
        $('.task-tree').find('.task-node-execution-time').remove()
    }

    taskTreeFocusedElement = $(this).parent() as JQuery<HTMLDivElement>
    taskTreeFocusedElement.addClass('task-node-focused')

    $.getJSON("Home/GetTask", {
        id: taskTreeFocusedElement.attr('task-id')
    }, function (resp) {
        nameInput.val(resp.name)
        descriptionInput.val(resp.description)
        listOfExecutorsInput.val(resp.listOfExecutors)
        statusSelect.children('[selected]').removeAttr('selected')
        statusSelect.children().eq(resp.status).attr('selected', 'selected')
        dateOfRegistrationInput.val(resp.dateOfRegistration)
        dateOfCompleteInput.val(resp.dateOfComplete)
        plannedComplexityInput.val(resp.plannedComplexity)
        actualExecutionTimeInput.val(resp.actualExecutionTime)

        traverseChildren(resp, node => {
            taskTreeFocusedElement.parent().find(`[task-id=${node.id}]`)
                .append($('<span>', {
                    'text': `(${node.actualExecutionTime})`,
                    'class': 'text-light task-node-execution-time'
                }))
        });
    })
})

$('#CreateSubtask').on('click', function (event) {
    if (taskTreeFocusedElement === undefined) {
        showErrorAlert("TaskNotSelected");
        return
    }

    taskForm.prepend($('<input>', {
        'id': 'ParentId',
        'name': 'ParentId',
        'type': 'hidden',
        'value': taskTreeFocusedElement.attr('task-id')
    }))
})

$('#SaveTask').on('click', function (event) {
    if (taskTreeFocusedElement === undefined) {
        showErrorAlert("TaskNotSelected")
        return
    }

    taskForm.append($('<input>', {
        'id': 'task_Id',
        'name': 'task.Id',
        'type': 'hidden',
        'value': taskTreeFocusedElement.attr('task-id')
    }))
})

$('#DeleteTask').on('click', function (event) {
    if (taskTreeFocusedElement === undefined) {
        showErrorAlert("TaskNotSelected")
        event.preventDefault()
        return
    }

    $('#delete-task-form').append($('<input>', {
        'id': 'id',
        'name': 'id',
        'type': 'hidden',
        'form': 'delete-task-form',
        'value': taskTreeFocusedElement.attr('task-id')
    }))
})

$('#ClearData').on('click', function (event) {
    $.each(inputsArray, (_, val) => {
        val.val('')
        val.parent().children('span').children().remove()
    })
    statusSelect.children('[selected]').removeAttr('selected')

    $('.task-tree').find('.task-node-execution-time').remove()
    if (taskTreeFocusedElement !== undefined) {
        taskTreeFocusedElement.removeClass('task-node-focused')
        taskTreeFocusedElement = undefined
    }
})
