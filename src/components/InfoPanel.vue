<template>
  <div>
    <b-sidebar
      id="sidepanel"
      title="Useful information"
      right
      backdrop-variant="transparent"
      backdrop
      shadow=true
      v-model="isPanelOpen">
      <div class="px-3 py-2">
        <p>
          Cras mattis consectetur purus sit amet fermentum. Cras justo odio, dapibus ac facilisis
          in, egestas eget quam. Morbi leo risus, porta ac consectetur ac, vestibulum at eros.
        </p>
      </div>
    </b-sidebar>
  </div>
</template>

<script>
import SidebarPanel from 'bootstrap-vue'

export default {
  name: "InfoPanel",
  components: { SidebarPanel },
  data () {
    return {
      isPanelOpen: false
    }
  },
  methods: {
    onNodeClick () {
      if (!this.isPanelOpen) {
        this.isPanelOpen = true
      }
    }
  }
}
</script>

<style scoped>

</style>
