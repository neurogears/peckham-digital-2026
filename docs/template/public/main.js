import WorkflowContainer from "./workflow.js"

export default {
    defaultTheme: 'light',
    iconLinks: [{
        icon: 'github',
        href: 'https://github.com/neurogears/peckham-digital-2026',
        title: 'GitHub'
    }],
    start: () => {
        WorkflowContainer.init();
    }
}
